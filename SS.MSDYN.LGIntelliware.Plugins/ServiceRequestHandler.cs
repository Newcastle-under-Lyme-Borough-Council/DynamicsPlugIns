using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using SS.MSDYN.LGIntelliware.Plugins.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace SS.MSDYN.LGIntelliware.Plugins
{
    // Runs when a form record is saved: creates/links a case and keeps the case in line with the form.
    public class ServiceRequestHandler : PluginBase
    {
        private const string LinkingFlag = "ServiceRequestHandler.LinkingCase";
        private const string IncidentContactColumn = "ss_customerid";
        private const string IncidentServiceRequestColumn = "ss_servicerequestid";
        private const string IncidentChildReferenceColumn = "nulbc_childreference";
        private const string IncidentSubjectColumn = "ss_subjectid";
        private const string IncidentFormNameColumn = "nulbc_formname";
        private const string IncidentCaseStatusFinalColumn = "nulbc_casestatusfinal";
        private const string StatusCaseTable = "nulbc_statuscase";
        private const string StatusCaseLookupColumn = "nulbc_statuscase";
        private const string StatusCaseCustomerStatusColumn = "nulbc_customerstatus";
        private const string StatusFilterColumn = "nulbc_statusfilter";
        private const string StatusCaseFinalStatusColumn = "nulbc_finalstatus";

        // Form columns that receive the customer status from the status case.
        private static readonly string[] CustomerStatusColumns =
        {
            "nulbc_customercasestatus"
        };

        // Form columns that hold the contact, in order of preference (the first one found on the table is used).
        private static readonly string[] ContactColumnCandidates =
        {
            "nulbc_customersearch"
        };

        // Columns with the same name on the form and the case, copied form -> case.
        private static readonly string[] StandardFields =
        {
            "nulbc_firstname",
            "nulbc_lastname",
            "nulbc_emailaddress",
            "nulbc_phonenumber",
            "nulbc_address",
            "nulbc_property",
            "nulbc_datesubmitted",
            "nulbc_description",
            "nulbc_datasource",
            "nulbc_externalreference",
            "nulbc_internalnotification",
            "nulbc_internalnotificationemail",
            "nulbc_internalnotificationurl",
            "ss_subjectid",
            "nulbc_customersearch",
            "nulbc_queue",
            "nulbc_datecompleted",
        };

        // Form column -> differently named case column.
        private static readonly KeyValuePair<string, string>[] MappedFields =
        {
            new KeyValuePair<string, string>("nulbc_customercasestatus", "nulbc_customerstatus"),
            new KeyValuePair<string, string>("nulbc_statuscase", "nulbc_statuscase"),
            new KeyValuePair<string, string>("nulbc_customersearch", "nulbc_contact"),
            new KeyValuePair<string, string>("nulbc_uprn", "nulbc_property"),
        };

        // A form column and the case column it is copied to.
        private class FieldPair
        {
            public string Form { get; set; }
            public string Incident { get; set; }
        }

        // Plugin constructor (no configuration needed).
        public ServiceRequestHandler() : base(typeof(ServiceRequestHandler))
        {
            RegisterEvents();
        }

        // Registers Execute as a PostOperation Update step.
        public void RegisterEvents()
        {
            RegisteredEvents.Add(new Tuple<int, string, string, Action<LocalPluginContext>>(
                PluginExecutionPipelineStage.PostOperation.GetHashCode(), PluginExecutionMessageName.UPDATE, string.Empty, Execute));
        }

        // Main entry point (Update of a form record):
        // 1. Work out the column settings from the form's metadata and sync the status fields onto the form.
        // 2. Create and link a case if there isn't one yet.
        // 3. Copy standard fields, reference, subject name and final status onto the case.
        // 4. Update the case only if something changed.
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

                var formMeta = GetAttributes(service, target.LogicalName);

                var primaryNameColumn = formMeta.Values.FirstOrDefault(a => a.IsPrimaryName == true)?.LogicalName;

                var config = BuildConfig(target.LogicalName, formMeta, primaryNameColumn, trace);
                var pairs = BuildFieldPairs(config);

                SyncStatusCaseFields(service, context, target, formMeta, trace);

                var incidentTable = GetIncidentTableName(service, target.LogicalName, config, trace);
                var incidentMeta = GetAttributes(service, incidentTable);
                var usable = FilterUsablePairs(pairs, formMeta, incidentMeta, trace);

                var formColumns = new[] { config.ServiceConfigurationColumnLogicalName, config.IncidentIdColumnLogicalName }
                    .Concat(usable.Select(p => p.Form))
                    .Concat(new[] { primaryNameColumn })
                    .Concat(new[] { formMeta.ContainsKey(StatusCaseLookupColumn) ? StatusCaseLookupColumn : null })
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                var record = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(formColumns));

                var serviceConfigRef = record.GetAttributeValue<EntityReference>(config.ServiceConfigurationColumnLogicalName);
                if (serviceConfigRef == null || serviceConfigRef.Id == Guid.Empty)
                {
                    trace.Trace("Service Config not pre-filled.");
                    trace.Trace("Exited plugin");
                    return;
                }

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

                    var fullRecord = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(true));

                    var incidentId = DataverseHelper.CreateCaseGeneric(service, fullRecord, serviceConfiguration, subject, ToPluginConfig(config), trace);
                    trace.Trace("Case created: " + incidentId);

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

                var incidentColumns = usable.Select(p => p.Incident).ToList();
                if (incidentMeta.ContainsKey(IncidentServiceRequestColumn))
                {
                    incidentColumns.Add(IncidentServiceRequestColumn);
                }

                if (incidentMeta.ContainsKey(IncidentChildReferenceColumn))
                {
                    incidentColumns.Add(IncidentChildReferenceColumn);
                }

                if (incidentMeta.ContainsKey(IncidentSubjectColumn))
                {
                    incidentColumns.Add(IncidentSubjectColumn);
                }

                if (incidentMeta.ContainsKey(IncidentFormNameColumn))
                {
                    incidentColumns.Add(IncidentFormNameColumn);
                }

                if (incidentMeta.ContainsKey(StatusCaseLookupColumn))
                {
                    incidentColumns.Add(StatusCaseLookupColumn);
                }

                if (incidentMeta.ContainsKey(IncidentCaseStatusFinalColumn))
                {
                    incidentColumns.Add(IncidentCaseStatusFinalColumn);
                }

                var incident = service.Retrieve(caseRef.LogicalName, caseRef.Id,
                    new ColumnSet(incidentColumns.Distinct().ToArray()));

                var update = new Entity(caseRef.LogicalName, caseRef.Id);
                foreach (var p in usable)
                {
                    if (p.Incident == "nulbc_address")
                    {
                        var formLen = record.Contains(p.Form) && record[p.Form] is string fs ? fs.Length : -1;
                        var incLen = incident.Contains(p.Incident) && incident[p.Incident] is string isv ? isv.Length : -1;
                        trace.Trace("Address check: hasData=" + HasData(record, p.Form) + ", formLength=" + formLen + ", incidentLength=" + incLen);
                    }

                    if (!HasData(record, p.Form))
                    {
                        continue;
                    }

                    var formValue = record[p.Form];

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

                if (incidentMeta.TryGetValue(IncidentFormNameColumn, out var formNameMeta) &&
                    formNameMeta.IsValidForUpdate == true &&
                    (formNameMeta.AttributeType == AttributeTypeCode.String || formNameMeta.AttributeType == AttributeTypeCode.Memo))
                {
                    var subjectRef = incident.GetAttributeValue<EntityReference>(IncidentSubjectColumn);
                    if (subjectRef != null && subjectRef.Id != Guid.Empty)
                    {
                        var subjectRecord = service.Retrieve(subjectRef.LogicalName, subjectRef.Id, new ColumnSet(Subject.Title));
                        var subjectName = subjectRecord.GetAttributeValue<string>(Subject.Title);

                        if (!string.IsNullOrWhiteSpace(subjectName))
                        {
                            var formNameMax = GetMaxLength(formNameMeta);
                            if (formNameMax.HasValue && subjectName.Length > formNameMax.Value)
                            {
                                subjectName = subjectName.Substring(0, formNameMax.Value);
                            }

                            var currentFormName = incident.Contains(IncidentFormNameColumn) ? incident[IncidentFormNameColumn] as string : null;
                            if (!string.Equals(currentFormName, subjectName, StringComparison.Ordinal))
                            {
                                update[IncidentFormNameColumn] = subjectName;
                                trace.Trace("Changed: " + IncidentSubjectColumn + " (name) -> " + IncidentFormNameColumn);
                            }
                        }
                        else
                        {
                            trace.Trace("Subject '" + subjectRef.Id + "' has no title - '" + IncidentFormNameColumn + "' not set.");
                        }
                    }
                    else
                    {
                        trace.Trace("Incident has no subject - '" + IncidentFormNameColumn + "' not set.");
                    }
                }
                else
                {
                    trace.Trace("'" + IncidentFormNameColumn + "' not set: the column is missing, not updatable or not text.");
                }

                if (incidentMeta.TryGetValue(IncidentCaseStatusFinalColumn, out var finalMeta) &&
                    finalMeta.IsValidForUpdate == true &&
                    (finalMeta.AttributeType == AttributeTypeCode.Boolean || finalMeta.AttributeType == AttributeTypeCode.Picklist))
                {
                    var finalStatusRef = record.GetAttributeValue<EntityReference>(StatusCaseLookupColumn)
                        ?? incident.GetAttributeValue<EntityReference>(StatusCaseLookupColumn);

                    if (finalStatusRef != null && finalStatusRef.Id != Guid.Empty)
                    {
                        var statusCaseRow = service.Retrieve(finalStatusRef.LogicalName, finalStatusRef.Id, new ColumnSet(StatusCaseFinalStatusColumn));
                        var finalValue = ConvertYesNo(
                            statusCaseRow.Contains(StatusCaseFinalStatusColumn) ? statusCaseRow[StatusCaseFinalStatusColumn] : null,
                            finalMeta);

                        if (finalValue != null)
                        {
                            var currentFinal = incident.Contains(IncidentCaseStatusFinalColumn) ? incident[IncidentCaseStatusFinalColumn] : null;
                            if (!ValuesEqual(finalValue, currentFinal))
                            {
                                update[IncidentCaseStatusFinalColumn] = finalValue;
                                trace.Trace("Changed: " + StatusCaseFinalStatusColumn + " (status case) -> " + IncidentCaseStatusFinalColumn);
                            }
                        }
                        else
                        {
                            trace.Trace("Status case '" + finalStatusRef.Id + "' has no usable '" + StatusCaseFinalStatusColumn + "' value - '" + IncidentCaseStatusFinalColumn + "' not set.");
                        }
                    }
                    else
                    {
                        trace.Trace("No status case on the record - '" + IncidentCaseStatusFinalColumn + "' not set.");
                    }
                }
                else
                {
                    trace.Trace("'" + IncidentCaseStatusFinalColumn + "' not set: the column is missing, not updatable or not yes/no.");
                }

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

        // When the status case changes on the form, copies its customer status and status filter onto the form.
        private static void SyncStatusCaseFields(
            IOrganizationService service,
            IPluginExecutionContext context,
            Entity target,
            Dictionary<string, AttributeMetadata> formMeta,
            ITracingService trace)
        {
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

            context.SharedVariables[LinkingFlag] = true;
            service.Update(update);
            trace.Trace("Status fields synced from " + StatusCaseTable + ".");
        }

        // Converts a status case value to the form column's type (choice as is, text uses the label).
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

        // Converts a yes/no value between a Two Options column and a Yes/No choice (1/0).
        private static object ConvertYesNo(object value, AttributeMetadata destination)
        {
            int? number = null;
            if (value is bool flag)
            {
                number = flag ? 1 : 0;
            }
            else if (value is OptionSetValue option)
            {
                number = option.Value;
            }

            if (number == null) return null;

            if (destination.AttributeType == AttributeTypeCode.Boolean)
            {
                return number.Value == 1;
            }

            if (destination.AttributeType == AttributeTypeCode.Picklist)
            {
                return new OptionSetValue(number.Value);
            }

            return null;
        }

        // True if this save was caused by the plugin's own update of the form record.
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

        // Copies the settings into the type CreateCaseGeneric expects.
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

        // Works out the column settings from the form table's metadata, so no step configuration is needed.
        private static ServiceRequestConfig BuildConfig(string tableName, Dictionary<string, AttributeMetadata> formMeta, string primaryNameColumn, ITracingService trace)
        {
            var lookups = formMeta.Values
                .OfType<LookupAttributeMetadata>()
                .Where(l => l.IsValidForRead == true)
                .OrderBy(l => l.LogicalName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var serviceConfigColumn = FindLookupTo(lookups, ServiceConfiguration.TableName, false, trace);
            var incidentColumn = FindLookupTo(lookups, Case.TableName, false, trace);

            if (serviceConfigColumn == null)
            {
                throw new InvalidPluginExecutionException("No lookup to the service configuration table was found on '" + tableName + "'.");
            }

            if (incidentColumn == null)
            {
                throw new InvalidPluginExecutionException("No lookup to the case table was found on '" + tableName + "'.");
            }

            var contactColumn = ContactColumnCandidates.FirstOrDefault(c => formMeta.ContainsKey(c))
                ?? FindLookupTo(lookups, "contact", true, trace);

            trace.Trace("Config built from metadata for '" + tableName + "': serviceConfig=" + serviceConfigColumn +
                ", case=" + incidentColumn + ", contact=" + (contactColumn ?? "(none)") + ", reference=" + (primaryNameColumn ?? "(none)"));

            return new ServiceRequestConfig
            {
                TableName = tableName,
                ServiceConfigurationColumnLogicalName = serviceConfigColumn,
                IncidentIdColumnLogicalName = incidentColumn,
                ContactColumnLogicalName = contactColumn,
                ReferenceNumberColumnLogicalName = primaryNameColumn
            };
        }

        // Finds the lookup column that points at the given table (null if none, or if several and requireSingle is set).
        private static string FindLookupTo(List<LookupAttributeMetadata> lookups, string targetTable, bool requireSingle, ITracingService trace)
        {
            var matches = lookups
                .Where(l => l.Targets != null && l.Targets.Contains(targetTable, StringComparer.OrdinalIgnoreCase))
                .Select(l => l.LogicalName)
                .ToList();

            if (matches.Count == 0)
            {
                return null;
            }

            if (matches.Count > 1)
            {
                trace.Trace("Several lookups point at '" + targetTable + "': " + string.Join(", ", matches) + (requireSingle ? " - none used." : " - using " + matches[0] + "."));
                return requireSingle ? null : matches[0];
            }

            return matches[0];
        }

        // Finds the case table from the form's case lookup (defaults to ss_incident).
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

        // Builds the full list of form -> case column pairs to copy.
        private static List<FieldPair> BuildFieldPairs(ServiceRequestConfig config)
        {
            var pairs = StandardFields
                .Select(f => new FieldPair { Form = f, Incident = f })
                .ToList();

            pairs.AddRange(MappedFields.Select(m => new FieldPair { Form = m.Key, Incident = m.Value }));

            if (!string.IsNullOrWhiteSpace(config.ContactColumnLogicalName))
            {
                pairs.Add(new FieldPair { Form = config.ContactColumnLogicalName, Incident = IncidentContactColumn });
            }

            return pairs;
        }

        // Loads a table's column metadata, keyed by logical name.
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

        // Keeps only pairs whose columns exist on both tables, are updatable and have compatible types.
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

        // True if two column types can be copied between each other (same type, lookups, or text).
        private static bool TypesCompatible(AttributeTypeCode? a, AttributeTypeCode? b)
        {
            if (a == b) return true;

            bool IsLookup(AttributeTypeCode? t) => t == AttributeTypeCode.Lookup || t == AttributeTypeCode.Customer;

            bool IsText(AttributeTypeCode? t) => t == AttributeTypeCode.String || t == AttributeTypeCode.Memo;

            return (IsLookup(a) && IsLookup(b)) || (IsText(a) && IsText(b));
        }

        // Maximum length of a text column (null if not text).
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

        // True if the column has a non-empty value.
        private static bool HasData(Entity record, string column)
        {
            if (!record.Contains(column) || record[column] == null) return false;
            if (record[column] is string s && string.IsNullOrWhiteSpace(s)) return false;
            return true;
        }

        // Compares two column values (lookups, choices, money and plain values).
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