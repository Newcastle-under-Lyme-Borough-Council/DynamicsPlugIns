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
    // If the form record has no case yet, create a new case and link it.
    // Copies standardised fields from the form, into the case - updates each time the form is saved.
    // The case updatin triggers QueueItemHandler
    // Configuration source uses the step's own Unsecure Configuration if one is provided, otherwise the JSON on the-
    // PostCreate step registered for the same table. (the step can be reigstered, just not enabled).
    //
    // When nulbc_statuscase is changed on the form, the customer status and status filter are also copied onto the
    // form from the matching nulbc_statuscase row (before the form is synced to the case).
    //
    // Single-line and multi-line text columns can be synced with each other. Text longer than the case column's
    // maximum length is trimmed to fit.
    //
    // The form table's primary column (the case reference) is copied into nulbc_childreference on the case.
    //
    // NEW: The title of the linked subject (ss_subjectid) is copied into nulbc_formsubject on the form record.
    // This runs PreOperation on Create and Update (ExecuteSubjectText), so it is saved with the record itself.
    public class ServiceRequestHandler : PluginBase
    {
        private const string PostCreateTypeName = "SS.MSDYN.LGIntelliware.Plugins.PostCreate";

        // Shared variable set while this plugin makes an update to the form record itself (linking the new case, or syncing
        // the status fields), so the update it causes doesn't run the plugin a second time.
        private const string LinkingFlag = "ServiceRequestHandler.LinkingCase";

        // Column on the incident that holds the contact. The form's contact column comes from the config.
        private const string IncidentContactColumn = "ss_customerid";

        // Lookup on the incident that points back at the form record. QueueItemHandler uses it as the queue item's Object, so it must be set before the queue item is created.
        private const string IncidentServiceRequestColumn = "ss_servicerequestid";

        // Column on the incident that receives the form table's primary column value (the case reference).
        private const string IncidentChildReferenceColumn = "nulbc_childreference";

        // Status case table, and the columns on it that are copied onto the form.
        private const string StatusCaseTable = "nulbc_statuscase";
        private const string StatusCaseLookupColumn = "nulbc_statuscase";
        private const string StatusCaseCustomerStatusColumn = "nulbc_customerstatus";
        private const string StatusFilterColumn = "nulbc_statusfilter";

        // NEW: Subject lookup on the form, and the text column its title is copied into.
        private const string FormSubjectLookupColumn = "ss_subjectid";
        private const string FormSubjectTextColumn = "nulbc_formname";

        // Whichever of these exists on the form table gets the customer status.
        private static readonly string[] CustomerStatusColumns =
        {
            "nulbc_customercasestatus"
        };

        // Standardised fields with the same logical name on the form table and the incident.
        private static readonly string[] StandardFields =
        {
            "nulbc_firstname",
            "nulbc_lastname",
            "nulbc_emailaddress",
            "nulbc_phonenumber",
            "nulbc_address",
            "nulbc_property",
            "nulbc_description",
            "nulbc_datesubmitted",      // QueueItemHandler requires this field to be triggered
            "nulbc_datasource",
            "nulbc_externalreference",
            "nulbc_internalnotification",
            "nulbc_internalnotificationemail",
            "nulbc_internalnotificationurl",
            "ss_subjectid",
            "nulbc_customersearch",
            "nulbc_queue",
            "nulbc_datecompleted",
            "nulbc_formname"
            // Leave out these values unless we need?
            // "nulbc_status", "nulbc_datecompleted",
            // "nulbc_queue", "nulbc_queueitem", "nulbc_casereference", "ss_casenumber", "nulbc_synceddate",
        };

        private static readonly KeyValuePair<string, string>[] MappedFields =
        {
            // Customer status
            new KeyValuePair<string, string>("nulbc_customercasestatus", "nulbc_customerstatus"),

            // Status case
            new KeyValuePair<string, string>("nulbc_statuscase", "nulbc_statuscase"),

            new KeyValuePair<string, string>("nulbc_customersearch", "nulbc_contact"),

            new KeyValuePair<string, string>("nulbc_uprn", "nulbc_property"),
        };

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly string _unsecureConfiguration;

        // Form column & incident column pairs.
        private class FieldPair
        {
            public string Form { get; set; }
            public string Incident { get; set; }
        }

        public ServiceRequestHandler(string unsecureConfiguration)
            : base(typeof(ServiceRequestHandler), unsecureConfiguration)
        {
            _unsecureConfiguration = unsecureConfiguration;
            RegisterEvents();
        }

        public ServiceRequestHandler() : base(typeof(ServiceRequestHandler))
        {
            RegisterEvents();
        }

        public void RegisterEvents()
        {
            // Update trigger only. Runs when a user saves a form record.
            RegisteredEvents.Add(new Tuple<int, string, string, Action<LocalPluginContext>>(
                PluginExecutionPipelineStage.PostOperation.GetHashCode(), PluginExecutionMessageName.UPDATE, string.Empty, Execute));

            // NEW: Copies the subject's title into nulbc_formsubject. PreOperation, so it is saved with the record (no extra update).
            RegisteredEvents.Add(new Tuple<int, string, string, Action<LocalPluginContext>>(
                PluginExecutionPipelineStage.PreOperation.GetHashCode(), PluginExecutionMessageName.CREATE, string.Empty, ExecuteSubjectText));

            RegisteredEvents.Add(new Tuple<int, string, string, Action<LocalPluginContext>>(
                PluginExecutionPipelineStage.PreOperation.GetHashCode(), PluginExecutionMessageName.UPDATE, string.Empty, ExecuteSubjectText));
        }

        // NEW: PreOperation (Create and Update).
        // Copies the title of the linked subject into nulbc_formsubject on the form record.
        // On Update it only does anything when ss_subjectid is part of the save. If the subject is cleared, the text is cleared too.
        protected void ExecuteSubjectText(LocalPluginContext localContext)
        {
            if (localContext == null)
            {
                throw new ArgumentNullException(nameof(localContext));
            }

            var context = localContext.PluginExecutionContext;
            var service = localContext.OrganizationService;
            var trace = localContext.TracingService;

            trace.Trace("ServiceRequestHandler (subject text) triggered. Table: " + context.PrimaryEntityName + ", Message: " + context.MessageName + ", Depth: " + context.Depth);

            try
            {
                if (!context.InputParameters.Contains(ContextInputParameters.TARGET) ||
                    !(context.InputParameters[ContextInputParameters.TARGET] is Entity target))
                {
                    trace.Trace("Exited plugin");
                    return;
                }

                // Only act when the subject is part of this save.
                if (!target.Contains(FormSubjectLookupColumn))
                {
                    trace.Trace("'" + FormSubjectLookupColumn + "' not in this save.");
                    trace.Trace("Exited plugin");
                    return;
                }

                // The destination must exist on this table, be settable and be a text column.
                var textMeta = GetAttribute(service, target.LogicalName, FormSubjectTextColumn);
                if (textMeta == null ||
                    (textMeta.AttributeType != AttributeTypeCode.String && textMeta.AttributeType != AttributeTypeCode.Memo))
                {
                    trace.Trace("'" + FormSubjectTextColumn + "' not set: missing or not a text column.");
                    trace.Trace("Exited plugin");
                    return;
                }

                string title = null;

                var subjectRef = target.GetAttributeValue<EntityReference>(FormSubjectLookupColumn);
                if (subjectRef != null && subjectRef.Id != Guid.Empty)
                {
                    var subject = service.Retrieve(Subject.TableName, subjectRef.Id, new ColumnSet(Subject.Title));
                    title = subject.GetAttributeValue<string>(Subject.Title);

                    // Trim to the column's maximum length so the save doesn't fail.
                    var max = GetMaxLength(textMeta);
                    if (title != null && max.HasValue && title.Length > max.Value)
                    {
                        trace.Trace("Trimmed subject text from " + title.Length + " to " + max.Value + " characters.");
                        title = title.Substring(0, max.Value);
                    }
                }

                // PreOperation: changing the Target is saved with the record, so no separate update is needed.
                target[FormSubjectTextColumn] = title;
                trace.Trace("'" + FormSubjectTextColumn + "' set to: " + (title ?? "(null)"));
                trace.Trace("Exited plugin");
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new InvalidPluginExecutionException("Fault exception occurred executing ServiceRequestHandler (subject text): " + ex + ".");
            }
            catch (InvalidPluginExecutionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An exception occurred executing ServiceRequestHandler (subject text): " + ex + ".");
            }
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

            if (context.Depth > 2)
            {
                trace.Trace("Exited plugin");
                return;
            }

            // If the update this plugin makes to the form record (linking a new case, syncing status fields) isn't from a user, skip it.
            if (IsLinkingUpdate(context))
            {
                trace.Trace("Triggered by this plugin updating the form record.");
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

                // Get the table/column settings for this form table.
                var config = LoadConfig(service, target.LogicalName, trace);

                // Work out which standardised fields can be copied.
                var pairs = BuildFieldPairs(config);
                var formMeta = GetAttributes(service, target.LogicalName);

                // The form table's primary column (the case reference).
                var primaryNameColumn = formMeta.Values.FirstOrDefault(a => a.IsPrimaryName == true)?.LogicalName;

                // If the status case was changed, copy its customer status + status filter onto the form.
                // Done before the form is read below, so the case is synced with the new values.
                SyncStatusCaseFields(service, context, target, formMeta, trace);

                var incidentTable = GetIncidentTableName(service, target.LogicalName, config, trace);
                var incidentMeta = GetAttributes(service, incidentTable);
                var usable = FilterUsablePairs(pairs, formMeta, incidentMeta, trace);

                // Read the form record.
                var formColumns = new[] { config.ServiceConfigurationColumnLogicalName, config.IncidentIdColumnLogicalName }
                    .Concat(usable.Select(p => p.Form))
                    .Concat(new[] { primaryNameColumn })
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
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

                // Does the case already exist for this form record?
                var caseRef = record.GetAttributeValue<EntityReference>(config.IncidentIdColumnLogicalName);
                if (caseRef == null || caseRef.Id == Guid.Empty)
                {
                    trace.Trace("No case exists yet for this record, creating new record.");

                    var serviceConfigurations = DataverseHelper.RetrieveServiceConfiguration(service, ServiceConfiguration.TableName, serviceConfigRef.Id, new ColumnSet(ServiceConfiguration.Subject, ServiceConfiguration.Name));
                    if (serviceConfigurations == null || serviceConfigurations.Entities == null || serviceConfigurations.Entities.Count == 0)
                    {
                        throw new InvalidPluginExecutionException("There are no active service configuration found for the selected record.");
                    }

                    var serviceConfiguration = serviceConfigurations.Entities[0];
                    if (!serviceConfiguration.Attributes.ContainsKey(ServiceConfiguration.Subject) || serviceConfiguration.Attributes[ServiceConfiguration.Subject] == null)
                    {
                        throw new InvalidPluginExecutionException("Selected record does not have subject configuration.");
                    }

                    var title = serviceConfiguration.GetAttributeValue<string>(ServiceConfiguration.Subject);
                    var subjects = DataverseHelper.RetrieveSubject(service, Subject.TableName, title, new ColumnSet(Subject.Title));
                    if (subjects == null || subjects.Entities == null || subjects.Entities.Count == 0)
                    {
                        throw new InvalidPluginExecutionException("There are no active subject found for the selected record.");
                    }

                    var subject = subjects.Entities[0];

                    // PostCreate received the whole new record; here the full record is read so the helper has every column.
                    var fullRecord = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(true));

                    // Create an Incident (case)
                    var incidentId = DataverseHelper.CreateCaseGeneric(service, fullRecord, serviceConfiguration, subject, ToPluginConfig(config), trace);
                    trace.Trace("Case created: " + incidentId);

                    // Link it to the form record
                    var entityToUpdate = new Entity(target.LogicalName)
                    {
                        Id = target.Id
                    };
                    entityToUpdate.Attributes.Add(config.IncidentIdColumnLogicalName, new EntityReference(Case.TableName, incidentId));

                    context.SharedVariables[LinkingFlag] = true;
                    DataverseHelper.Update(service, entityToUpdate);
                    trace.Trace("Case linked to the form record.");

                    caseRef = new EntityReference(Case.TableName, incidentId);
                }

                // Bring the incident in line with the form.
                var incidentColumns = usable.Select(p => p.Incident).ToList();
                if (incidentMeta.ContainsKey(IncidentServiceRequestColumn))
                {
                    incidentColumns.Add(IncidentServiceRequestColumn);
                }

                if (incidentMeta.ContainsKey(IncidentChildReferenceColumn))
                {
                    incidentColumns.Add(IncidentChildReferenceColumn);
                }

                var incident = service.Retrieve(caseRef.LogicalName, caseRef.Id,
                    new ColumnSet(incidentColumns.Distinct().ToArray()));

                // Build an update containing only the standardised fields that have data and differ.
                var update = new Entity(caseRef.LogicalName, caseRef.Id);
                foreach (var p in usable)
                {

                    // Debug whether the address field is actually filled in
                    if (p.Incident == "nulbc_address")
                    {
                        var formLen = record.Contains(p.Form) && record[p.Form] is string fs ? fs.Length : -1;
                        var incLen = incident.Contains(p.Incident) && incident[p.Incident] is string isv ? isv.Length : -1;
                        trace.Trace("Address check: hasData=" + HasData(record, p.Form) + ", formLength=" + formLen + ", incidentLength=" + incLen);
                    }

                    if (!HasData(record, p.Form))
                    {
                        continue;   // Never blank out an incident field
                    }

                    var formValue = record[p.Form];

                    // Text copied between single-line and multi-line columns is trimmed to the incident column's maximum length.
                    if (formValue is string text)
                    {
                        var max = GetMaxLength(incidentMeta[p.Incident]);
                        if (max.HasValue && text.Length > max.Value)
                        {
                            trace.Trace("Trimmed '" + p.Form + "' from " + text.Length + " to " + max.Value + " characters for '" + p.Incident + "'.");
                            text = text.Substring(0, max.Value);
                        }

                        formValue = text;
                    }

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

                // Copy the form's primary column (case reference) into the child reference column on the incident.
                if (!string.IsNullOrWhiteSpace(primaryNameColumn) &&
                    incidentMeta.TryGetValue(IncidentChildReferenceColumn, out var childMeta) &&
                    childMeta.IsValidForUpdate == true &&
                    (childMeta.AttributeType == AttributeTypeCode.String || childMeta.AttributeType == AttributeTypeCode.Memo))
                {
                    var reference = record.GetAttributeValue<string>(primaryNameColumn);
                    if (!string.IsNullOrWhiteSpace(reference))
                    {
                        var childMax = GetMaxLength(childMeta);
                        if (childMax.HasValue && reference.Length > childMax.Value)
                        {
                            reference = reference.Substring(0, childMax.Value);
                        }

                        var currentReference = incident.Contains(IncidentChildReferenceColumn) ? incident[IncidentChildReferenceColumn] as string : null;
                        if (!string.Equals(currentReference, reference, StringComparison.Ordinal))
                        {
                            update[IncidentChildReferenceColumn] = reference;
                            trace.Trace("Changed: " + primaryNameColumn + " -> " + IncidentChildReferenceColumn);
                        }
                    }
                }
                else
                {
                    trace.Trace("'" + IncidentChildReferenceColumn + "' not set: no primary column found, or the column is missing, not updatable or not text.");
                }

                // Point the incident back at this form record.
                if (incidentMeta.TryGetValue(IncidentServiceRequestColumn, out var srMeta) &&
                    srMeta.IsValidForUpdate == true &&
                    AcceptsTable(srMeta, record.LogicalName))
                {
                    var currentLink = incident.GetAttributeValue<EntityReference>(IncidentServiceRequestColumn);
                    if (currentLink == null || currentLink.Id != record.Id)
                    {
                        update[IncidentServiceRequestColumn] = new EntityReference(record.LogicalName, record.Id);
                        trace.Trace("Changed: " + IncidentServiceRequestColumn + " -> " + record.LogicalName + " " + record.Id);
                    }
                }
                else
                {
                    trace.Trace("'" + IncidentServiceRequestColumn + "' not set: missing, not updatable, or doesn't accept '" + record.LogicalName + "'.");
                }

                // Update the incident - Triggers QueueItemHandler.
                if (update.Attributes.Count == 0)
                {
                    trace.Trace("Incident already in line with the form - no update is needed.");
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

        // When nulbc_statuscase is changed on the form, copies the matching customer status and
        // status filter from the nulbc_statuscase row onto the form record.
        private static void SyncStatusCaseFields(
            IOrganizationService service,
            IPluginExecutionContext context,
            Entity target,
            Dictionary<string, AttributeMetadata> formMeta,
            ITracingService trace)
        {
            // Only runs when the lookup is part of this save.
            if (!target.Contains(StatusCaseLookupColumn)) return;

            var customerCol = CustomerStatusColumns.FirstOrDefault(c =>
                formMeta.TryGetValue(c, out var m) && m.IsValidForUpdate == true);

            var filterCol = formMeta.TryGetValue(StatusFilterColumn, out var fm) && fm.IsValidForUpdate == true
                ? StatusFilterColumn
                : null;

            if (customerCol == null && filterCol == null)
            {
                trace.Trace("Status sync skipped: form has neither a customer status nor a status filter column.");
                return;
            }

            var statusRef = target.GetAttributeValue<EntityReference>(StatusCaseLookupColumn);
            var update = new Entity(target.LogicalName, target.Id);

            if (statusRef == null)
            {
                // Lookup cleared -> clear the synced columns too.
                if (customerCol != null) update[customerCol] = null;
                if (filterCol != null) update[filterCol] = null;
            }
            else
            {
                var statusCase = service.Retrieve(
                    StatusCaseTable,
                    statusRef.Id,
                    new ColumnSet(StatusCaseCustomerStatusColumn, StatusFilterColumn));

                if (customerCol != null)
                {
                    update[customerCol] = ConvertForColumn(statusCase, StatusCaseCustomerStatusColumn, formMeta[customerCol], trace);
                }

                if (filterCol != null)
                {
                    update[filterCol] = ConvertForColumn(statusCase, StatusFilterColumn, formMeta[filterCol], trace);
                }
            }

            // The update below re-triggers this plugin; the flag makes that nested run exit early.
            context.SharedVariables[LinkingFlag] = true;
            service.Update(update);
            trace.Trace("Status fields synced from " + StatusCaseTable + ".");
        }

        // Choice -> same-type choice is copied as is; choice -> text uses the label.
        private static object ConvertForColumn(Entity source, string sourceColumn, AttributeMetadata destination, ITracingService trace)
        {
            if (!source.Contains(sourceColumn) || source[sourceColumn] == null) return null;

            var value = source[sourceColumn];

            if (value is OptionSetValue && destination.AttributeType == AttributeTypeCode.Picklist)
            {
                return value;
            }

            if (destination.AttributeType == AttributeTypeCode.String || destination.AttributeType == AttributeTypeCode.Memo)
            {
                return source.FormattedValues.Contains(sourceColumn)
                    ? source.FormattedValues[sourceColumn]
                    : value.ToString();
            }

            trace.Trace("Status sync: can't convert '" + sourceColumn + "' to " + destination.AttributeType + " column '" + destination.LogicalName + "'.");
            return null;
        }

        // Links case to form record, so the update it causes doesn't run the plugin a second time.
        private static bool IsLinkingUpdate(IPluginExecutionContext context)
        {
            var parent = context.ParentContext;
            while (parent != null)
            {
                if (parent.SharedVariables.Contains(LinkingFlag))
                {
                    return true;
                }
                parent = parent.ParentContext;
            }
            return false;
        }

        // CreateCaseGeneric takes the PluginConfig type, so the config is copied across.
        private static PluginConfig ToPluginConfig(ServiceRequestConfig c)
        {
            return new PluginConfig
            {
                TableName = c.TableName,
                ContactColumnLogicalName = c.ContactColumnLogicalName,
                ReferenceNumberColumnLogicalName = c.ReferenceNumberColumnLogicalName,
                ServiceConfigurationColumnLogicalName = c.ServiceConfigurationColumnLogicalName,
                IncidentIdColumnLogicalName = c.IncidentIdColumnLogicalName
            };
        }

        // Uses the step's own Unsecure Configuration.
        private ServiceRequestConfig LoadConfig(IOrganizationService service, string tableName, ITracingService trace)
        {
            if (!string.IsNullOrWhiteSpace(_unsecureConfiguration))
            {
                var own = JsonSerializer.Deserialize<ServiceRequestConfig>(_unsecureConfiguration, JsonOptions);
                if (own != null &&
                    !string.IsNullOrWhiteSpace(own.ServiceConfigurationColumnLogicalName) &&
                    !string.IsNullOrWhiteSpace(own.IncidentIdColumnLogicalName))
                {
                    trace.Trace("Config read from this step's unsecure configuration.");
                    return own;
                }

                throw new InvalidPluginExecutionException(
                    "The unsecure configuration on this step is incomplete.");
            }

            return LoadConfigFromPostCreateStep(service, tableName, trace);
        }

        // Reads the Unsecure Configuration JSON from the PostCreate Create step registered for this table.
        private static ServiceRequestConfig LoadConfigFromPostCreateStep(IOrganizationService service, string tableName, ITracingService trace)
        {
            var query = new QueryExpression("sdkmessageprocessingstep")
            {
                ColumnSet = new ColumnSet("configuration", "name"),
                TopCount = 1
            };

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
                    "No configuration was found for '" + tableName + "' (no unsecure configuration on this step and no PostCreate step).");
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

        // The standardised fields are copied from the form to the incident: same-name fields, mapped fields, and the contact.
        private static List<FieldPair> BuildFieldPairs(ServiceRequestConfig config)
        {
            var pairs = StandardFields
                .Select(f => new FieldPair { Form = f, Incident = f })
                .ToList();

            // Fields named differently on the form and the incident.
            pairs.AddRange(MappedFields.Select(m => new FieldPair { Form = m.Key, Incident = m.Value }));

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

        // NEW: Metadata for a single column, or null if the column doesn't exist or can't be set.
        private static AttributeMetadata GetAttribute(IOrganizationService service, string tableName, string columnName)
        {
            try
            {
                var response = (RetrieveAttributeResponse)service.Execute(new RetrieveAttributeRequest
                {
                    EntityLogicalName = tableName,
                    LogicalName = columnName,
                    RetrieveAsIfPublished = false
                });

                var attribute = response.AttributeMetadata;
                return attribute.IsValidForCreate == true || attribute.IsValidForUpdate == true ? attribute : null;
            }
            catch (FaultException<OrganizationServiceFault>)
            {
                return null;
            }
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
                    trace.Trace("Not synced: '" + p.Form + "' is not on the form table.");
                    continue;
                }

                if (!incidentMeta.TryGetValue(p.Incident, out var incAttr) || incAttr.IsValidForUpdate != true)
                {
                    trace.Trace("Not synced: '" + p.Incident + "' is not on the incident table or can't be updated.");
                    continue;
                }

                if (!TypesCompatible(formAttr.AttributeType, incAttr.AttributeType))
                {
                    trace.Trace("Not synced: '" + p.Form + "' (" + formAttr.AttributeType + ") vs '" + p.Incident + "' (" + incAttr.AttributeType + ") types differ.");
                    continue;
                }

                usable.Add(p);
            }

            trace.Trace("Synced: " + string.Join(", ", usable.Select(p => p.Form + (p.Form == p.Incident ? "" : "->" + p.Incident))));
            return usable;
        }

        private static bool TypesCompatible(AttributeTypeCode? a, AttributeTypeCode? b)
        {
            if (a == b) return true;

            // Lookup-style columns can be copied between each other.
            bool IsLookup(AttributeTypeCode? t) => t == AttributeTypeCode.Lookup || t == AttributeTypeCode.Customer;

            // Single-line and multi-line text columns can be copied between each other.
            bool IsText(AttributeTypeCode? t) => t == AttributeTypeCode.String || t == AttributeTypeCode.Memo;

            return (IsLookup(a) && IsLookup(b)) || (IsText(a) && IsText(b));
        }

        // Maximum number of characters a text column accepts (null if not a text column).
        private static int? GetMaxLength(AttributeMetadata attribute)
        {
            if (attribute is StringAttributeMetadata s) return s.MaxLength;
            if (attribute is MemoAttributeMetadata m) return m.MaxLength;
            return null;
        }

        // True if a lookup column accepts records from the given table.
        private static bool AcceptsTable(AttributeMetadata attribute, string tableName)
        {
            var lookup = attribute as LookupAttributeMetadata;
            if (lookup == null || lookup.Targets == null || lookup.Targets.Length == 0)
            {
                return true;
            }

            return lookup.Targets.Contains(tableName);
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