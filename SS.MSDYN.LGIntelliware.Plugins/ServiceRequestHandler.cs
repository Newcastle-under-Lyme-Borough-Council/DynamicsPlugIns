using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using SS.MSDYN.LGIntelliware.Plugins.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text.Json;

namespace SS.MSDYN.LGIntelliware.Plugins
{
    // Copies the standardised fields from a form record to its linked service request (ss_incident) record.
    // The incident update is what triggers QueueItemHandler, which should create the queue item?

    // The table/column settings are read from the PostCreate step registered for the same table.
    // Register an Update step on each form table.
    public class ServiceRequestHandler : PluginBase
    {
        private const string PostCreateTypeName = "SS.MSDYN.LGIntelliware.Plugins.PostCreate";

        // Column on the incident that holds the contact. The form's contact column comes from the config
        // ContactColumnLogicalName, so it is mapped explicitly below.
        private const string IncidentContactColumn = "ss_customerid";

        // Standardised fields that are copied from the form to the incident.
        private static readonly string[] StandardFields =
        {
            "nulbc_firstname",
            "nulbc_lastname",
            "nulbc_emailaddress",
            "nulbc_phonenumber",
            "nulbc_address",
            "nulbc_property",
            "nulbc_description",
            "nulbc_datesubmitted",      // QueueItemHandler won't run without this
            "nulbc_datasource",
            "nulbc_externalreference",
            "nulbc_internalnotification",
            "nulbc_internalnotificationemail",
            "nulbc_internalnotificationurl",
            "ss_subjectid",
            // Leave out these values unless we need?
            // "nulbc_status", "nulbc_casestatus", "nulbc_customerstatus", "nulbc_datecompleted",
            // "nulbc_queue", "nulbc_queueitem", "nulbc_casereference", "ss_casenumber", "nulbc_synceddate",
        };

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        // Form column & incident column pairs.
        private class FieldPair
        {
            public string Form { get; set; }
            public string Incident { get; set; }
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
            // Update trigger only. Runs when a user edits a record, and when PostCreate links the new case.
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

            // Depth 1 = User edits record, depth 2 = PostCreate linking the new case. Both should run after User edits.
            if (context.Depth > 2)
            {
                trace.Trace("Exited plugin");
                return;
            }

            try
            {
                if (!context.InputParameters.Contains(ContextInputParameters.TARGET) ||
                    !(context.InputParameters[ContextInputParameters.TARGET] is Entity target))
                {
                    trace.Trace("Exited plugin");
                    return;
                }

                // Get the table/column settings from the PostCreate step for this table.
                var config = LoadConfigFromPostCreateStep(service, target.LogicalName, trace);

                // Work out which standardised fields can be copied.
                var pairs = BuildFieldPairs(config);
                var formMeta = GetAttributes(service, target.LogicalName);
                var incidentTable = GetIncidentTableName(service, target.LogicalName, config, trace);
                var incidentMeta = GetAttributes(service, incidentTable);
                var usable = FilterUsablePairs(pairs, formMeta, incidentMeta, trace);

                // Read the form record.
                var formColumns = new[] { config.ServiceConfigurationColumnLogicalName, config.IncidentIdColumnLogicalName }
                    .Concat(usable.Select(p => p.Form))
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Distinct()
                    .ToArray();

                var record = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(formColumns));

                // Only continue when the Service Config lookup has been filled in.
                var serviceConfigRef = record.GetAttributeValue<EntityReference>(config.ServiceConfigurationColumnLogicalName);
                if (serviceConfigRef == null || serviceConfigRef.Id == Guid.Empty)
                {
                    trace.Trace("Service Config not pre-filled.");
                    trace.Trace("Exited plugin");
                    return;
                }

                // Need the linked service request (incident) record.
                var caseRef = record.GetAttributeValue<EntityReference>(config.IncidentIdColumnLogicalName);
                if (caseRef == null || caseRef.Id == Guid.Empty)
                {
                    trace.Trace("No service request (case) has been linked yet.");
                    trace.Trace("Exited plugin");
                    return;
                }

                var incident = service.Retrieve(caseRef.LogicalName, caseRef.Id,
                    new ColumnSet(usable.Select(p => p.Incident).Distinct().ToArray()));

                // Build an update containing only the standardised fields that have data and differ.
                var update = new Entity(caseRef.LogicalName, caseRef.Id);
                foreach (var p in usable)
                {
                    if (!HasData(record, p.Form))
                    {
                        continue;   // Never blank out an incident field
                    }

                    var formValue = record[p.Form];

                    // A lookup must point at a table the incident column accepts.
                    if (formValue is EntityReference er &&
                        incidentMeta[p.Incident] is LookupAttributeMetadata lookup &&
                        lookup.Targets != null && lookup.Targets.Length > 0 &&
                        !lookup.Targets.Contains(er.LogicalName))
                    {
                        trace.Trace("Skipped '" + p.Form + "' - lookup target '" + er.LogicalName + "' not accepted by '" + p.Incident + "'.");
                        continue;
                    }

                    var incidentValue = incident.Contains(p.Incident) ? incident[p.Incident] : null;
                    if (ValuesEqual(formValue, incidentValue))
                    {
                        continue;
                    }

                    update[p.Incident] = formValue;
                    trace.Trace("Changed: " + p.Form + " -> " + p.Incident);
                }

                // Update the incident. Should trigger QueueItemHandler?
                if (update.Attributes.Count == 0)
                {
                    trace.Trace("Incident already in line with the form - no update needed.");
                    trace.Trace("Exited plugin");
                    return;
                }

                service.Update(update);
                trace.Trace("Incident " + caseRef.Id + " updated with " + update.Attributes.Count + " field(s).");

                trace.Trace("Exited plugin");
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

        // The incident table is ss_incident; reads from the case lookup's target - so it stays correct if that ever changes.
        private static string GetIncidentTableName(IOrganizationService service, string formTable, ServiceRequestConfig config, ITracingService trace)
        {
            var attribute = (RetrieveAttributeResponse)service.Execute(new RetrieveAttributeRequest
            {
                EntityLogicalName = formTable,
                LogicalName = config.IncidentIdColumnLogicalName,
                RetrieveAsIfPublished = false
            });

            var lookup = attribute.AttributeMetadata as LookupAttributeMetadata;
            var table = lookup?.Targets?.FirstOrDefault() ?? "ss_incident";
            trace.Trace("Service request table: " + table);
            return table;
        }

        // The standardised fields, plus the contact (whose form column comes from the config).
        private static List<FieldPair> BuildFieldPairs(ServiceRequestConfig config)
        {
            var pairs = StandardFields
                .Select(f => new FieldPair { Form = f, Incident = f })
                .ToList();

            if (!string.IsNullOrWhiteSpace(config.ContactColumnLogicalName))
            {
                pairs.Add(new FieldPair { Form = config.ContactColumnLogicalName, Incident = IncidentContactColumn });
            }

            return pairs;
        }

        private static Dictionary<string, AttributeMetadata> GetAttributes(IOrganizationService service, string tableName)
        {
            var response = (RetrieveEntityResponse)service.Execute(new RetrieveEntityRequest
            {
                LogicalName = tableName,
                EntityFilters = EntityFilters.Attributes,
                RetrieveAsIfPublished = false
            });

            return response.EntityMetadata.Attributes.ToDictionary(a => a.LogicalName, a => a, StringComparer.OrdinalIgnoreCase);
        }

        // Keeps only pairs that exist on both tables, can be updated on the incident, and have matching types.
        private static List<FieldPair> FilterUsablePairs(
            List<FieldPair> pairs,
            Dictionary<string, AttributeMetadata> formMeta,
            Dictionary<string, AttributeMetadata> incidentMeta,
            ITracingService trace)
        {
            var usable = new List<FieldPair>();

            foreach (var p in pairs)
            {
                if (!formMeta.TryGetValue(p.Form, out var formAttr) || formAttr.IsValidForRead != true)
                {
                    trace.Trace("Not copied: '" + p.Form + "' is not on the form table.");
                    continue;
                }

                if (!incidentMeta.TryGetValue(p.Incident, out var incAttr) || incAttr.IsValidForUpdate != true)
                {
                    trace.Trace("Not copied: '" + p.Incident + "' is not on the incident table or can't be updated.");
                    continue;
                }

                if (!TypesCompatible(formAttr.AttributeType, incAttr.AttributeType))
                {
                    trace.Trace("Not copied: '" + p.Form + "' (" + formAttr.AttributeType + ") vs '" + p.Incident + "' (" + incAttr.AttributeType + ") types differ.");
                    continue;
                }

                usable.Add(p);
            }

            trace.Trace("Fields that will be synced: " + string.Join(", ", usable.Select(p => p.Form + (p.Form == p.Incident ? "" : "->" + p.Incident))));
            return usable;
        }

        private static bool TypesCompatible(AttributeTypeCode? a, AttributeTypeCode? b)
        {
            if (a == b) return true;

            // Lookup-style columns can be copied between each other (target table is checked separately).
            bool IsLookup(AttributeTypeCode? t) => t == AttributeTypeCode.Lookup || t == AttributeTypeCode.Customer;
            return IsLookup(a) && IsLookup(b);
        }

        private static bool HasData(Entity record, string column)
        {
            if (!record.Contains(column) || record[column] == null) return false;
            if (record[column] is string s && string.IsNullOrWhiteSpace(s)) return false;
            return true;
        }

        // Ensure values are correct
        private static bool ValuesEqual(object a, object b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;

            if (a is EntityReference ra && b is EntityReference rb) return ra.Id == rb.Id;
            if (a is OptionSetValue oa && b is OptionSetValue ob) return oa.Value == ob.Value;
            if (a is Money ma && b is Money mb) return ma.Value == mb.Value;

            return a.Equals(b);
        }
    }
}