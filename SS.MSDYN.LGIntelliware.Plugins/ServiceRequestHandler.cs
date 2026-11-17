using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
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

    // The table/column settings are read from the PostCreate step registered for the same table.
    // The summary is built from every custom column on the form table that has data, marking which
    // are mandatory; mandatory columns with no data are flagged as missing.
    public class ServiceRequestHandler : PluginBase
    {
        private const string PostCreateTypeName = "SS.MSDYN.LGIntelliware.Plugins.PostCreate";
        // Key used to hand the summary to the queue plugin (same transaction only).
        public const string SummarySharedVariable = "CaseSummary";

        // Columns to leave out of the summary
        // The config columns (service config, case, reference, contact) aren't included here.
        private static readonly HashSet<string> ExcludedColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "nulbc_internalnotification",
            "nulbc_statusfilter",
            "nulbc_datasource",
            "nulbc_selectedaddress",
        };

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        // A form column that can appear in the summary.
        private class SummaryField
        {
            public string LogicalName { get; set; }
            public string Label { get; set; }
            public bool IsMandatory { get; set; }
        }

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
            // Update trigger only. Will run if a user creates a new record, or if a user edits an exisitng record.
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

            trace.Trace("ServiceRequestHandler triggered. Table: " + context.PrimaryEntityName + ", Message: " + context.MessageName + ", Depth: " + context.Depth);

            // Depth 1 = a user edit, depth 2 = PostCreate linking the new case. Both should run.
            if (context.Depth > 2)
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
                var coreColumns = Array.FindAll(
                    new[]
                    {
                        config.ServiceConfigurationColumnLogicalName,
                        config.IncidentIdColumnLogicalName,
                        config.ReferenceNumberColumnLogicalName,
                        config.ContactColumnLogicalName
                    },
                    c => !string.IsNullOrWhiteSpace(c));

                // Work out every mandatory / non-mandatory column on this form table.
                var fields = GetSummaryFields(service, target.LogicalName, coreColumns, trace);

                var columns = coreColumns.Concat(fields.Select(f => f.LogicalName)).Distinct().ToArray();
                var record = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(columns));
                trace.Trace("Fetched " + columns.Length + " columns.");

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

                var summary = BuildSummary(record, serviceConfig, config, fields, trace);
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

        // Reads the Unsecure Configuration JSON from the active PostCreate Create step registered in XRM Toolbox.
        // In other words - Reads the ServiceConfig on the current form record.
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

        // Reads the table's metadata and returns every field on the form.
        private static List<SummaryField> GetSummaryFields(IOrganizationService service, string tableName, string[] coreColumns, ITracingService trace)
        {
            var response = (RetrieveEntityResponse)service.Execute(new RetrieveEntityRequest
            {
                LogicalName = tableName,
                EntityFilters = EntityFilters.Attributes,
                RetrieveAsIfPublished = false
            });

            var core = new HashSet<string>(coreColumns, StringComparer.OrdinalIgnoreCase);
            var fields = new List<SummaryField>();

            foreach (var a in response.EntityMetadata.Attributes)
            {
                if (a.IsCustomAttribute != true) continue;                    // skips system columns (createdon, owner, status...)
                if (a.IsValidForRead != true) continue;
                if (!string.IsNullOrEmpty(a.AttributeOf)) continue;           // skips virtual "...name" companion columns
                if (a.IsPrimaryId == true) continue;
                if (core.Contains(a.LogicalName) || ExcludedColumns.Contains(a.LogicalName)) continue;

                var type = a.AttributeType;
                if (type == AttributeTypeCode.ManagedProperty ||
                    type == AttributeTypeCode.Uniqueidentifier ||
                    type == AttributeTypeCode.EntityName ||
                    type == AttributeTypeCode.CalendarRules ||
                    type == AttributeTypeCode.PartyList ||
                    (type == AttributeTypeCode.Virtual && !(a is MultiSelectPicklistAttributeMetadata)))
                {
                    continue;
                }

                var level = a.RequiredLevel?.Value;
                fields.Add(new SummaryField
                {
                    LogicalName = a.LogicalName,
                    Label = a.DisplayName?.UserLocalizedLabel?.Label ?? a.LogicalName,
                    IsMandatory = level == AttributeRequiredLevel.ApplicationRequired ||
                                  level == AttributeRequiredLevel.SystemRequired
                });
            }

            trace.Trace("Summary columns found on '" + tableName + "': " + fields.Count +
                        " (mandatory: " + fields.Count(f => f.IsMandatory) + ")");
            trace.Trace("Summary column names: " + string.Join(", ", fields.Select(f => f.LogicalName)));
            return fields;
        }

        // Create/Build the summary from the record.
        private static string BuildSummary(Entity record, Entity serviceConfig, ServiceRequestConfig config, List<SummaryField> fields, ITracingService trace)
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

            // Creates list of fields in trace logs. Mandatory fields are listed first, then non-mandatory fields, both alphabetically by label. Only fields with data are included in the summary.
            var listed = fields
                .Where(f => f.IsMandatory || HasData(record, f.LogicalName))
                .OrderByDescending(f => f.IsMandatory)
                .ThenBy(f => f.Label)
                .ToList();

            if (listed.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Fields:");
                foreach (var f in listed)
                {
                    if (HasData(record, f.LogicalName))
                    {
                        sb.AppendLine("- " + f.Label + ": " + FormatValue(record, f.LogicalName));
                    }
                    else
                    {
                        trace.Trace("Mandatory '" + f.LogicalName + "' has no data.");
                        sb.AppendLine("- " + f.Label + ": [missing]");
                    }
                }
            }

            return sb.ToString().TrimEnd();
        }

        private static bool HasData(Entity record, string column)
        {
            if (!record.Contains(column) || record[column] == null) return false;
            if (record[column] is string s && string.IsNullOrWhiteSpace(s)) return false;
            return true;
        }

        // Turns a column value into readable text.
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