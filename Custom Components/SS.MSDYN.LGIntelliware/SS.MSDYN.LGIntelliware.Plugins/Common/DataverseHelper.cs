using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.MSDYN.LGIntelliware.Plugins
{
    /// <summary>
    /// Helper class to provide a set of common functions.
    /// </summary>
    public static class DataverseHelper
    {
        public static EntityCollection RetrieveServiceConfiguration(this IOrganizationService service, ITracingService tracingService, string entityName, Guid serviceConfigurationId, ColumnSet columnSet)
        {
            try
            {
                tracingService.Trace("{0}", "Starting execute 'RetrieveServiceConfiguration' method.");
                QueryExpression query = new QueryExpression(entityName)
                {
                    ColumnSet = columnSet,
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                         new ConditionExpression(ServiceConfigurationTableColumnNames.ServiceConfiguration, ConditionOperator.Equal, serviceConfigurationId),
                         new ConditionExpression(ServiceConfigurationTableColumnNames.Status, ConditionOperator.Equal, StateCode.Active.GetHashCode()) ,
                         new ConditionExpression(ServiceConfigurationTableColumnNames.StatusReason, ConditionOperator.Equal, StatusCode.Active.GetHashCode())
                        }
                    }
                };
                tracingService.Trace("{0}", "Exiting from 'RetrieveServiceConfiguration' method after execution.");
                return service.RetrieveMultiple(query);
            }
            catch (Exception ex)
            {
                tracingService.Trace("An exception occured executing RetrieveServiceConfiguration: {0}.", ex.ToString());
                throw new InvalidPluginExecutionException("Fault exception occured executing RetrieveServiceConfiguration: " + ex.Message + ".");
            }
        }
        public static EntityCollection RetrieveSubject(this IOrganizationService service, ITracingService tracingService, string entityName, string title, ColumnSet columnSet)
        {
            try
            {
                tracingService.Trace("{0}", "Starting execute 'RetrieveSubject' method.");
                QueryExpression query = new QueryExpression(entityName)
                {
                    ColumnSet = columnSet,
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                         new ConditionExpression(SubjectTableColumnNames.Title, ConditionOperator.Equal, title)
                        }
                    }
                };
                tracingService.Trace("{0}", "Exiting from 'RetrieveSubject' method after execution.");
                return service.RetrieveMultiple(query);
            }
            catch (Exception ex)
            {
                tracingService.Trace("An exception occured executing RetrieveSubject: {0}.", ex.ToString());
                throw new InvalidPluginExecutionException("Fault exception occured executing RetrieveSubject: " + ex.Message + ".");
            }
        }
        public static Guid CreateIncident(this IOrganizationService service, ITracingService tracingService, Entity entity, Entity serviceConfiguration, Entity subject)
        {
            try
            {
                tracingService.Trace("Starting execute 'CreateIncident' method.");
                var entityToCreate = new Entity(IncidentTableColumnNames.TableName);
                if (serviceConfiguration.Attributes.ContainsKey(ServiceConfigurationTableColumnNames.ServiceConfiguration) && serviceConfiguration.Attributes[ServiceConfigurationTableColumnNames.ServiceConfiguration] != null)
                {
                    tracingService.Trace("Adding 'Service' attribute in incident creation.");
                    entityToCreate.Attributes.Add(IncidentTableColumnNames.Service, new EntityReference(ServiceConfigurationTableColumnNames.TableName, serviceConfiguration.GetAttributeValue<Guid>(ServiceConfigurationTableColumnNames.ServiceConfiguration)));
                }

                if ((serviceConfiguration.Attributes.ContainsKey(ServiceConfigurationTableColumnNames.ServiceConfiguration) && serviceConfiguration.Attributes[ServiceConfigurationTableColumnNames.ServiceConfiguration] != null) && (entity.Attributes.ContainsKey(ServiceRequestTableColumnNames.ReferenceNumber) && entity.Attributes[ServiceRequestTableColumnNames.ReferenceNumber] != null))
                {
                    tracingService.Trace("Adding 'Incident Title' attribute in incident creation.");
                    entityToCreate.Attributes.Add(IncidentTableColumnNames.CaseTitle, entity.Attributes[ServiceRequestTableColumnNames.ReferenceNumber] + " - " + serviceConfiguration.Attributes[ServiceConfigurationTableColumnNames.Name] + " - " + " Service Request ");
                }

                tracingService.Trace("Adding 'CaseType' attribute in incident creation.");
                entityToCreate.Attributes.Add(IncidentTableColumnNames.CaseType, new OptionSetValue(IncidentType.Request.GetHashCode()));

                if (entity.Attributes.ContainsKey(ServiceRequestTableColumnNames.Customer) && entity.Attributes[ServiceRequestTableColumnNames.Customer] != null)
                {
                    tracingService.Trace("Adding 'Customer' attribute in incident creation.");
                    entityToCreate.Attributes.Add(IncidentTableColumnNames.Customer, new EntityReference(ContactTableColumnNames.TableName, entity.GetAttributeValue<EntityReference>(ServiceRequestTableColumnNames.Customer).Id));
                }

                if (entity.Attributes.ContainsKey(ServiceRequestTableColumnNames.Description) && entity.Attributes[ServiceRequestTableColumnNames.Description] != null)
                {
                    tracingService.Trace("Adding 'Description' attribute in incident creation.");
                    entityToCreate.Attributes.Add(IncidentTableColumnNames.Description, entity.GetAttributeValue<string>(ServiceRequestTableColumnNames.Description));
                }

                if (entity.Attributes.ContainsKey(ServiceRequestTableColumnNames.Owner) && entity.Attributes[ServiceRequestTableColumnNames.Owner] != null)
                {
                    tracingService.Trace("Adding 'Owner' attribute in incident creation.");
                    entityToCreate.Attributes.Add(IncidentTableColumnNames.Owner, new EntityReference(SystemUserTableColumnNames.TableName, entity.GetAttributeValue<EntityReference>(ServiceRequestTableColumnNames.Owner).Id));
                }

                tracingService.Trace("Adding 'Priority' attribute in incident creation.");
                entityToCreate.Attributes.Add(IncidentTableColumnNames.Priority, new OptionSetValue(IncidentPriority.Normal.GetHashCode()));

                if (entity.Attributes.ContainsKey(ServiceRequestTableColumnNames.ReportedBy) && entity.Attributes[ServiceRequestTableColumnNames.ReportedBy] != null)
                {
                    tracingService.Trace("Adding 'ReportedBy' attribute in incident creation.");
                    entityToCreate.Attributes.Add(IncidentTableColumnNames.ReportedBy, new EntityReference(ContactTableColumnNames.TableName, entity.GetAttributeValue<EntityReference>(ServiceRequestTableColumnNames.ReportedBy).Id));
                }

                tracingService.Trace("Adding 'ServiceRequest' attribute in incident creation.");
                entityToCreate.Attributes.Add(IncidentTableColumnNames.ServiceRequest, new EntityReference(entity.LogicalName, entity.Id));


                if (subject.Attributes.ContainsKey(SubjectTableColumnNames.Subject) && subject.Attributes[SubjectTableColumnNames.Subject] != null)
                {
                    tracingService.Trace("Adding 'Subject' attribute in incident creation.");
                    entityToCreate.Attributes.Add(IncidentTableColumnNames.Subject, new EntityReference(SubjectTableColumnNames.TableName, subject.GetAttributeValue<Guid>(SubjectTableColumnNames.Subject)));
                }

                if (entity.Attributes.ContainsKey(ServiceRequestTableColumnNames.ReferenceNumber) && entity.Attributes[ServiceRequestTableColumnNames.ReferenceNumber] != null)
                {
                    tracingService.Trace("Adding 'CaseNumber' attribute in incident creation.");
                    entityToCreate.Attributes.Add(IncidentTableColumnNames.CaseNumber, entity.GetAttributeValue<string>(ServiceRequestTableColumnNames.ReferenceNumber));
                }

                if (serviceConfiguration.Attributes.ContainsKey(ServiceConfigurationTableColumnNames.Subject) && serviceConfiguration.Attributes[ServiceConfigurationTableColumnNames.Subject] != null)
                {
                    if (serviceConfiguration.GetAttributeValue<string>(ServiceConfigurationTableColumnNames.Subject).ToUpper().Equals(ServiceConfigurationSubjects.TAXILICENCE.ToUpper()))
                    {
                        tracingService.Trace("Adding 'ServiceRequestStatus' attribute in incident creation.");
                        entityToCreate.Attributes.Add(IncidentTableColumnNames.ServiceRequestStatus, ServiceRequestStatuses.OPEN);
                    }
                }

                if (entity.Attributes.ContainsKey(ServiceRequestTableColumnNames.SourceType) && entity.Attributes[ServiceRequestTableColumnNames.SourceType] != null)
                {
                    tracingService.Trace("Adding 'SourceType' attribute in incident creation.");
                    var missedBinSourceType = entity.GetAttributeValue<OptionSetValue>(ServiceRequestTableColumnNames.SourceType).Value;
                    if (missedBinSourceType == ServiceRequestSourceType.Phone.GetHashCode())
                        entityToCreate.Attributes.Add(IncidentTableColumnNames.Origin, new OptionSetValue(IncidentOrigin.Phone.GetHashCode()));
                    else if (missedBinSourceType == ServiceRequestSourceType.Web.GetHashCode())
                        entityToCreate.Attributes.Add(IncidentTableColumnNames.Origin, new OptionSetValue(IncidentOrigin.Web.GetHashCode()));
                    else if (missedBinSourceType == ServiceRequestSourceType.Email.GetHashCode())
                        entityToCreate.Attributes.Add(IncidentTableColumnNames.Origin, new OptionSetValue(IncidentOrigin.Email.GetHashCode()));
                    else if (missedBinSourceType == ServiceRequestSourceType.Portal.GetHashCode())
                        entityToCreate.Attributes.Add(IncidentTableColumnNames.Origin, new OptionSetValue(IncidentOrigin.Portal.GetHashCode()));
                }
                tracingService.Trace("Exiting from 'CreateIncident' method after execution.");
                return service.Create(entityToCreate); ;
            }
            catch (Exception ex)
            {
                tracingService.Trace("An exception occured executing CreateIncident: {0}.", ex.ToString());
                throw new InvalidPluginExecutionException("Fault exception occured executing CreateIncident: " + ex.Message + ".");
            }
        }
        public static void Update(this IOrganizationService service, ITracingService tracingService, Entity entityToUpdate)
        {
            try
            {
                tracingService.Trace("{0}", "Starting execute 'Update' method.");
                service.Update(entityToUpdate);
                tracingService.Trace("Exiting from 'Update' method after execution.");
            }
            catch (Exception ex)
            {
                tracingService.Trace("An exception occured executing Update: {0}.", ex.ToString());
                throw new InvalidPluginExecutionException("Fault exception occured executing Update: " + ex.Message + ".");
            }
        }
    }
}

