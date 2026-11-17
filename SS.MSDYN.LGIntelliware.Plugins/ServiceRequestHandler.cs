using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using SS.MSDYN.LGIntelliware.Plugins.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Text.Json;

namespace SS.MSDYN.LGIntelliware.Plugins
{
    /// <summary>
    /// Case summary plugin. Register Create + Update steps (Post-operation, Synchronous) on each form
    /// table with an EMPTY unsecure configuration. The plugin reads the table / column settings from
    /// the existing PostCreate step registered for the same table.
    ///
    /// Create step: Execution Order 2 (after PostCreate has created and linked the case).
    /// Update step: set Filtering Attributes to the columns that should trigger a refresh.
    /// Run in User's Context: use an account that can read plug-in step records (e.g. a system admin
    /// / service account), as the plugin queries the step configuration.
    /// </summary>
    public class ServiceRequestHandler : PluginBase
    {
        private const string PostCreateTypeName = "SS.MSDYN.LGIntelliware.Plugins.PostCreate";
        // Key used to hand the summary to the queue plugin (same transaction only).
        public const string SummarySharedVariable = "CaseSummary";

        /// <summary>
        /// ADD NEW SUMMARY FIELDS HERE.
        /// Key = label shown in the summary, Value = column logical name (the same on every form table).
        /// A field is skipped if the table doesn't have the column or the value is empty/null.
        /// </summary>
        private static readonly KeyValuePair<string, string>[] AdditionalFields =
        {
            new KeyValuePair<string, string>("Council Tax reference", "nulbc_counciltaxreference"),
            // new KeyValuePair<string, string>("Label shown in summary", "logical_name"),
        };

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public ServiceRequestHandler(string unsecureConfiguration)
            : base(typeof(ServiceRequestHandler), unsecureConfiguration)
        {
            RegisterEvents();
        }

        public ServiceRequestHandler() : base(typeof(ServiceRequestHandler))
        {
            RegisterEvents();
        }

        public void RegisterEvents()
        {
            // Plugin triggers
            RegisteredEvents.Add(new Tuple<int, string, string, Action<LocalPluginContext>>(
                PluginExecutionPipelineStage.PostOperation.GetHashCode(), PluginExecutionMessageName.CREATE, string.Empty, Execute));
            RegisteredEvents.Add(new Tuple<int, string, string, Action<LocalPluginContext>>(
                PluginExecutionPipelineStage.PostOperation.GetHashCode(), PluginExecutionMessageName.UPDATE, string.Empty, Execute));
        }

        protected void Execute(LocalPluginContext localContext)
        {
            if (localContext == null)
            {
                throw new ArgumentNullException(nameof(localContext));
            }

            var context = localContext.PluginExecutionContext;
            var service = localContext.OrganizationService;
            var trace = localContext.TracingService;

            trace.Trace("ServiceRequestHandler start. Table: " + context.PrimaryEntityName + ", Message: " + context.MessageName + ", Depth: " + context.Depth);

            if (context.Depth > 2 || (context.MessageName == PluginExecutionMessageName.UPDATE && context.Depth > 1))
            {
                return;
            }

            try
            {
                if (!context.InputParameters.Contains(ContextInputParameters.TARGET) ||
                    !(context.InputParameters[ContextInputParameters.TARGET] is Entity target))
                {
                    return;
                }

                // Get the table/column settings from the PostCreate step for this table.
                var config = LoadConfigFromPostCreateStep(service, target.LogicalName, trace);

                // Columns that are always needed.
                var columns = new List<string>(Array.FindAll(
                    new[]
                    {
                        config.ServiceConfigurationColumnLogicalName,
                        config.IncidentIdColumnLogicalName,
                        config.ReferenceNumberColumnLogicalName,
                        config.ContactColumnLogicalName
                    },
                    c => !string.IsNullOrWhiteSpace(c)));

                // Add the additional summary fields, but only those that exist on this table.
                var tableColumns = GetTableColumns(service, target.LogicalName);
                foreach (var field in AdditionalFields)
                {
                    if (tableColumns.Contains(field.Value))
                    {
                        columns.Add(field.Value);
                    }
                    else
                    {
                        trace.Trace("Column '" + field.Value + "' not on '" + target.LogicalName + "' - skipped.");
                    }
                }

                var record = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(columns.Distinct().ToArray()));
                trace.Trace("Fetched columns: " + string.Join(", ", columns));

                // Only continue when the Service Config lookup has been filled in.
                var serviceConfigRef = record.GetAttributeValue<EntityReference>(config.ServiceConfigurationColumnLogicalName);
                if (serviceConfigRef == null || serviceConfigRef.Id == Guid.Empty)
                {
                    trace.Trace("Service Config not pre-filled.");
                    return;
                }

                // Need a case to link the summary to
                var caseRef = record.GetAttributeValue<EntityReference>(config.IncidentIdColumnLogicalName);
                if (caseRef == null || caseRef.Id == Guid.Empty)
                {
                    trace.Trace("No case has been linked yet.");
                    return;
                }

                var serviceConfig = service.Retrieve(serviceConfigRef.LogicalName, serviceConfigRef.Id,
                    new ColumnSet(ServiceConfiguration.Name, ServiceConfiguration.Subject));

                var summary = BuildSummary(record, serviceConfig, config, trace);
                trace.Trace("Summary: " + summary);

                // Output for the queue plugin
                context.SharedVariables[SummarySharedVariable] = summary;

                var caseUpdate = new Entity(caseRef.LogicalName, caseRef.Id);
                service.Update(caseUpdate);
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new InvalidPluginExecutionException("Fault exception occurred executing ServiceRequestHandler: " + ex + ".");
            }
            catch (InvalidPluginExecutionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An exception occurred executing ServiceRequestHandler: " + ex + ".");
            }
        }

        /// <summary>
        /// Reads the Unsecure Configuration JSON from the active PostCreate Create step
        /// registered on the given table.
        /// </summary>
        private static ServiceRequestConfig LoadConfigFromPostCreateStep(IOrganizationService service, string tableName, ITracingService trace)
        {
            var query = new QueryExpression("sdkmessageprocessingstep")
            {
                ColumnSet = new ColumnSet("configuration", "name"),
                TopCount = 1
            };
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);

            var pluginType = query.AddLink("plugintype", "plugintypeid", "plugintypeid");
            pluginType.LinkCriteria.AddCondition("typename", ConditionOperator.Equal, PostCreateTypeName);

            var message = query.AddLink("sdkmessage", "sdkmessageid", "sdkmessageid");
            message.LinkCriteria.AddCondition("name", ConditionOperator.Equal, "Create");

            var filter = query.AddLink("sdkmessagefilter", "sdkmessagefilterid", "sdkmessagefilterid");
            filter.LinkCriteria.AddCondition("primaryobjecttypecode", ConditionOperator.Equal, tableName);

            var results = service.RetrieveMultiple(query);
            if (results.Entities.Count == 0)
            {
                throw new InvalidPluginExecutionException(
                    "No PostCreate step configuration was found for '" + tableName + "'.");
            }

            var step = results.Entities[0];
            trace.Trace("Config read from step: " + step.GetAttributeValue<string>("name"));

            var json = step.GetAttributeValue<string>("configuration");
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new InvalidPluginExecutionException(
                    "The PostCreate step for '" + tableName + "' has no configuration.");
            }

            var config = JsonSerializer.Deserialize<ServiceRequestConfig>(json, JsonOptions);
            if (config == null ||
                string.IsNullOrWhiteSpace(config.ServiceConfigurationColumnLogicalName) ||
                string.IsNullOrWhiteSpace(config.IncidentIdColumnLogicalName))
            {
                throw new InvalidPluginExecutionException(
                    "The PostCreate step configuration for '" + tableName + "' is incomplete.");
            }

            return config;
        }

        /// <summary>Gets the logical names of all columns on a table, so missing columns can be skipped safely.</summary>
        private static HashSet<string> GetTableColumns(IOrganizationService service, string tableName)
        {
            var response = (RetrieveEntityResponse)service.Execute(new RetrieveEntityRequest
            {
                LogicalName = tableName,
                EntityFilters = Microsoft.Xrm.Sdk.Metadata.EntityFilters.Attributes,
                RetrieveAsIfPublished = false
            });

            return new HashSet<string>(
                response.EntityMetadata.Attributes.Select(a => a.LogicalName),
                StringComparer.OrdinalIgnoreCase);
        }

        // Create/Build the summary from the record.
        private static string BuildSummary(Entity record, Entity serviceConfig, ServiceRequestConfig config, ITracingService trace)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Form: " + config.TableName);

            if (!string.IsNullOrWhiteSpace(config.ReferenceNumberColumnLogicalName))
            {
                sb.AppendLine("Reference: " + record.GetAttributeValue<string>(config.ReferenceNumberColumnLogicalName));
            }

            if (!string.IsNullOrWhiteSpace(config.ContactColumnLogicalName))
            {
                var contact = record.GetAttributeValue<EntityReference>(config.ContactColumnLogicalName);
                sb.AppendLine("Contact: " + (contact?.Name ?? contact?.Id.ToString() ?? "Not provided"));
            }

            sb.AppendLine("Service: " + serviceConfig.GetAttributeValue<string>(ServiceConfiguration.Name));
            sb.AppendLine("Subject: " + serviceConfig.GetAttributeValue<string>(ServiceConfiguration.Subject));

            // Additional fields - only included when the column was fetched and has a value.
            foreach (var field in AdditionalFields)
            {
                if (!record.Contains(field.Value) || record[field.Value] == null)
                {
                    trace.Trace("'" + field.Value + "' is null/not present - not added to summary.");
                    continue;
                }

                var text = FormatValue(record, field.Value);
                if (string.IsNullOrWhiteSpace(text))
                {
                    trace.Trace("'" + field.Value + "' is empty - not added to summary.");
                    continue;
                }

                sb.AppendLine(field.Key + ": " + text);
            }

            return sb.ToString().TrimEnd();
        }

        /// <summary>Turns a column value into readable text (lookup names, option set labels, money, dates...).</summary>
        private static string FormatValue(Entity record, string column)
        {
            var value = record[column];

            if (value is EntityReference er)
            {
                return er.Name ?? er.Id.ToString();
            }

            if (record.FormattedValues.Contains(column))
            {
                return record.FormattedValues[column];
            }

            if (value is Money money)
            {
                return money.Value.ToString("0.00");
            }

            return value.ToString();
        }
    }
}