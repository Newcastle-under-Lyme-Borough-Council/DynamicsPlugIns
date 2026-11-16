using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using SS.MSDYN.LGIntelliware.Plugins.Common;
using System;
using System.ServiceModel;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

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
        // Column on the case that receives the summary (assumption - change to your real column).
        private const string CaseSummaryColumn = "ss_casesummary";
        // Key used to hand the summary to the queue plugin (same transaction only).
        public const string SummarySharedVariable = "CaseSummary";

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
            localContext.TracingService.Trace("Service request plugin executed.");
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
            localContext.TracingService.Trace("Run on update");
            try
            {
                if (!context.InputParameters.Contains(ContextInputParameters.TARGET) ||
                    !(context.InputParameters[ContextInputParameters.TARGET] is Entity target))
                {
                    return;
                }

                // Get the table/column settings from the PostCreate step for this table.
                var config = LoadConfigFromPostCreateStep(service, target.LogicalName, trace);

                // Get the full form record (Update targets only hold changed columns, and on Create - the case lookup was written by PostCreate after the Target was built).
                var columns = Array.FindAll(
                    new[]
                    {
                        config.ServiceConfigurationColumnLogicalName,
                        config.IncidentIdColumnLogicalName,
                        config.ReferenceNumberColumnLogicalName,
                        config.ContactColumnLogicalName
                    },
                    c => !string.IsNullOrWhiteSpace(c));

                var record = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(columns));

                localContext.TracingService.Trace("Fetched standardised columns");

                // Only continue when the Service Config lookup has been filled in.
                var serviceConfigRef = record.GetAttributeValue<EntityReference>(config.ServiceConfigurationColumnLogicalName);
                if (serviceConfigRef == null || serviceConfigRef.Id == Guid.Empty)
                {
                    trace.Trace("Service Config not filled - nothing to do.");
                    return;
                }

                // Need a case to write the summary to
                var caseRef = record.GetAttributeValue<EntityReference>(config.IncidentIdColumnLogicalName);
                if (caseRef == null || caseRef.Id == Guid.Empty)
                {
                    trace.Trace("No case linked yet - nothing to do.");
                    return;
                }

                var serviceConfig = service.Retrieve(serviceConfigRef.LogicalName, serviceConfigRef.Id,
                    new ColumnSet(ServiceConfiguration.Name, ServiceConfiguration.Subject));

                var summary = BuildSummary(record, serviceConfig, config);
                trace.Trace("Summary: " + summary);

                // Output for the queue plugin
                context.SharedVariables[SummarySharedVariable] = summary;

                var caseUpdate = new Entity(caseRef.LogicalName, caseRef.Id);
                caseUpdate[CaseSummaryColumn] = summary;
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
        private static PluginConfig LoadConfigFromPostCreateStep(IOrganizationService service, string tableName, ITracingService trace)
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
                    "No PostCreate step configuration was found for '" + tableName + "'. Please contact system administrator for more details");
            }

            var step = results.Entities[0];
            trace.Trace("Config read from step: " + step.GetAttributeValue<string>("name"));

            var json = step.GetAttributeValue<string>("configuration");
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new InvalidPluginExecutionException(
                    "The PostCreate step for '" + tableName + "' has no configuration. Please contact system administrator for more details");
            }

            var config = JsonSerializer.Deserialize<PluginConfig>(json, JsonOptions);
            if (config == null ||
                string.IsNullOrWhiteSpace(config.ServiceConfigurationColumnLogicalName) ||
                string.IsNullOrWhiteSpace(config.IncidentIdColumnLogicalName))
            {
                throw new InvalidPluginExecutionException(
                    "The PostCreate step configuration for '" + tableName + "' is incomplete. Please contact system administrator for more details");
            }

            return config;
        }

        private static string BuildSummary(Entity record, Entity serviceConfig, PluginConfig config)
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
            return sb.ToString().TrimEnd();
        }
    }
}