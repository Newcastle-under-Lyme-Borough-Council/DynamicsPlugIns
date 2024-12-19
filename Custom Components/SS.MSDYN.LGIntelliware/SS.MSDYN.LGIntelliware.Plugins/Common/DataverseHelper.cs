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
        public static EntityCollection RetrieveProperties(this IOrganizationService service, ITracingService tracingService, string entityName, string uprn, ColumnSet columnSet)
        {
            try
            {
                tracingService.Trace("{0}", "Starting execution of 'RetrieveProperties' method.");
                var allRecords = new EntityCollection();
                if (uprn != null)
                {
                    var query = new QueryExpression(entityName)
                    {
                        ColumnSet = columnSet,
                        Criteria = new FilterExpression
                        {
                            Conditions =
         {
             new ConditionExpression(PropertyTableColumnNames.Uprn, ConditionOperator.Equal, uprn)
         }
                        },
                        PageInfo = new PagingInfo
                        {
                            Count = 5000,
                            PageNumber = 1
                        }
                    };
                    do
                    {
                        var currentPage = service.RetrieveMultiple(query);
                        allRecords.Entities.AddRange(currentPage.Entities);
                        // Check if more records exist
                        if (currentPage.MoreRecords)
                        {
                            query.PageInfo.PageNumber++;
                            query.PageInfo.PagingCookie = currentPage.PagingCookie;
                        }
                        else
                        {
                            break;
                        }
                    } while (true);
                }
                tracingService.Trace("{0}", "Exiting from 'RetrieveProperties' method after execution.");
                return allRecords;
            }
            catch (Exception ex)
            {
                tracingService.Trace("An exception occurred while executing RetrieveProperties: {0}.", ex.ToString());
                throw new InvalidPluginExecutionException("An error occurred while retrieving properties: " + ex.Message + ".");
            }
        }
        public static void CreateContactProperty(this IOrganizationService service, ITracingService tracingService, EntityCollection properties, Guid contactId)
        {
            try
            {
                tracingService.Trace("{0}", "Starting execution of 'CreateContactProperty' method.");
                var totalProperties = properties.Entities.Count;
                var recordsPerPage = 500;
                var totalPages = (int)Math.Ceiling((double)totalProperties / recordsPerPage);
                for (var pageNumber = 0; pageNumber < totalPages; pageNumber++)
                {
                    // Create an ExecuteMultipleRequest object.
                    var multipleRequest = new ExecuteMultipleRequest()
                    {
                        Settings = new ExecuteMultipleSettings()
                        {
                            ContinueOnError = false,
                            ReturnResponses = true
                        },
                        Requests = new OrganizationRequestCollection()
                    };
                    var startIndex = pageNumber * recordsPerPage;
                    var endIndex = Math.Min(startIndex + recordsPerPage, totalProperties);
                    for (var index = startIndex; index < endIndex; index++)
                    {
                        var property = properties.Entities[index];
                        var contactproperty = new Entity(ContactPropertyTableColumnNames.TableName)
                        {
                            [ContactPropertyTableColumnNames.Property] = new EntityReference(PropertyTableColumnNames.TableName, property.Id),
                            [ContactPropertyTableColumnNames.Contact] = new EntityReference(ContactTableColumnNames.TableName, contactId),
                        };
                        multipleRequest.Requests.Add(new CreateRequest { Target = contactproperty });
                    }
                    var multipleResponse = (ExecuteMultipleResponse)service.Execute(multipleRequest);
                    foreach (var responseItem in multipleResponse.Responses)
                    {
                        if (responseItem.Fault != null)
                        {
                            tracingService.Trace("Error creating record in table `contactproperty`: {0}", responseItem.Fault.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                tracingService.Trace("An exception occurred while executing CreateContactProperty: {0}.", ex.ToString());
                throw new InvalidPluginExecutionException("An error occurred while creating records in table `contact property`: " + ex.Message + ".");
            }
        }
        public static EntityCollection RetrieveContacts(this IOrganizationService service, ITracingService tracingService, string entityName, string uprn, ColumnSet columnSet)
        {
            try
            {
                tracingService.Trace("{0}", "Starting execution of 'RetrieveContact' method.");
                var allRecords = new EntityCollection();
                if (uprn != null)
                {
                    var query = new QueryExpression(entityName)
                    {
                        ColumnSet = columnSet,
                        Criteria = new FilterExpression
                        {
                            Conditions =
         {
             new ConditionExpression(ContactTableColumnNames.Uprn, ConditionOperator.Equal, uprn)
         }
                        },
                        PageInfo = new PagingInfo
                        {
                            Count = 5000,
                            PageNumber = 1
                        }
                    };

                    do
                    {
                        var currentPage = service.RetrieveMultiple(query);
                        allRecords.Entities.AddRange(currentPage.Entities);
                        if (currentPage.MoreRecords)
                        {
                            query.PageInfo.PageNumber++;
                            query.PageInfo.PagingCookie = currentPage.PagingCookie;
                        }
                        else
                        {
                            break;
                        }
                    } while (true);
                }

                tracingService.Trace("{0}", "Exiting from 'RetrieveContacts' method after execution.");
                return allRecords;
            }
            catch (Exception ex)
            {
                tracingService.Trace("An exception occurred while executing RetrieveContacts: {0}.", ex.ToString());
                throw new InvalidPluginExecutionException("An error occurred while retrieving contacts: " + ex.Message + ".");
            }
        }
        public static void AddContactProperty(this IOrganizationService service, ITracingService tracingService, EntityCollection contacts, Guid propertyId)
        {
            try
            {
                tracingService.Trace("{0}", "Starting execution of 'AddContactProperty' method.");
                var totalcontacts = contacts.Entities.Count;
                var recordsPerPage = 500;
                var totalPages = (int)Math.Ceiling((double)totalcontacts / recordsPerPage);
                for (var pageNumber = 0; pageNumber < totalPages; pageNumber++)
                {
                    // Create an ExecuteMultipleRequest object.
                    var multipleRequest = new ExecuteMultipleRequest()
                    {
                        Settings = new ExecuteMultipleSettings()
                        {
                            ContinueOnError = false,
                            ReturnResponses = true
                        },
                        Requests = new OrganizationRequestCollection()
                    };

                    // Determine start and end index for current page
                    var startIndex = pageNumber * recordsPerPage;
                    var endIndex = Math.Min(startIndex + recordsPerPage, totalcontacts);

                    for (var index = startIndex; index < endIndex; index++)
                    {
                        var contact = contacts.Entities[index];
                        var contactproperty = new Entity(ContactPropertyTableColumnNames.TableName)
                        {
                            [ContactPropertyTableColumnNames.Property] = new EntityReference(PropertyTableColumnNames.TableName, propertyId),
                            [ContactPropertyTableColumnNames.Contact] = new EntityReference(ContactTableColumnNames.TableName, contact.Id),
                        };
                        multipleRequest.Requests.Add(new CreateRequest { Target = contactproperty });
                    }
                    var multipleResponse = (ExecuteMultipleResponse)service.Execute(multipleRequest);
                    foreach (var responseItem in multipleResponse.Responses)
                    {
                        if (responseItem.Fault != null)
                        {
                            tracingService.Trace("Error creating record in table `contactproperty`: {0}", responseItem.Fault.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                tracingService.Trace("An exception occurred while executing AddContactProperty: {0}.", ex.ToString());
                throw new InvalidPluginExecutionException("An error occurred while creating records in table `contact property`: " + ex.Message + ".");
            }
        }
        public static EntityCollection RetrieveContactProperties(this IOrganizationService service, ITracingService tracingService, string entityName, Guid contactId, ColumnSet columnSet)
        {
            try
            {
                tracingService.Trace("{0}", "Starting execution of 'RetrieveContactProperties' method.");
                var query = new QueryExpression(entityName)
                {
                    ColumnSet = columnSet,
                    Criteria = new FilterExpression
                    {
                        Conditions =
         {
             new ConditionExpression(ContactPropertyTableColumnNames.Contact, ConditionOperator.Equal, contactId)
         }
                    },
                    PageInfo = new PagingInfo
                    {
                        Count = 5000,
                        PageNumber = 1
                    }
                };
                var allRecords = new EntityCollection();
                do
                {
                    var currentPage = service.RetrieveMultiple(query);
                    allRecords.Entities.AddRange(currentPage.Entities);
                    if (currentPage.MoreRecords)
                    {
                        query.PageInfo.PageNumber++;
                        query.PageInfo.PagingCookie = currentPage.PagingCookie;
                    }
                    else
                    {
                        break;
                    }
                } while (true);

                tracingService.Trace("{0}", "Exiting from 'RetrieveContactProperties' method after execution.");
                return allRecords;
            }
            catch (Exception ex)
            {
                tracingService.Trace("An exception occurred while executing RetrieveContactProperties: {0}.", ex.ToString());
                throw new InvalidPluginExecutionException("An error occurred while retrieve contact properties: " + ex.Message + ".");
            }
        }
        public static void DeleteContactProperties(this IOrganizationService service, ITracingService tracingService, EntityCollection contactProperties)
        {
            try
            {
                tracingService.Trace("{0}", "Starting execution of 'DeleteContactProperties' method.");
                var totalContactProperties = contactProperties.Entities.Count;
                var recordsPerPage = 500;
                var totalPages = (int)Math.Ceiling((double)totalContactProperties / recordsPerPage);

                for (var pageNumber = 0; pageNumber < totalPages; pageNumber++)
                {
                    var multipleRequest = new ExecuteMultipleRequest()
                    {
                        Settings = new ExecuteMultipleSettings()
                        {
                            ContinueOnError = false,
                            ReturnResponses = true
                        },
                        Requests = new OrganizationRequestCollection()
                    };
                    var startIndex = pageNumber * recordsPerPage;
                    var endIndex = Math.Min(startIndex + recordsPerPage, totalContactProperties);
                    for (var index = startIndex; index < endIndex; index++)
                    {
                        var contact = contactProperties.Entities[index];
                        multipleRequest.Requests.Add(new DeleteRequest { Target = new EntityReference(ContactPropertyTableColumnNames.TableName, contact.Id) });
                    }
                    var multipleResponse = (ExecuteMultipleResponse)service.Execute(multipleRequest);
                    foreach (var responseItem in multipleResponse.Responses)
                    {
                        if (responseItem.Fault != null)
                        {
                            tracingService.Trace("Error deleting record in table `contactproperty`: {0}", responseItem.Fault.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                tracingService.Trace("An exception occurred while executing DeleteContactProperties: {0}.", ex.ToString());
                throw new InvalidPluginExecutionException("An error occurred while deleting records in table `contact property`: " + ex.Message + ".");
            }
        }
        public static EntityCollection RetrievePropertiesContact(this IOrganizationService service, ITracingService tracingService, string entityName, Guid propertyId, ColumnSet columnSet)
        {
            try
            {
                tracingService.Trace("{0}", "Starting execution of 'RetrievePropertiesContact' method.");
                var allRecords = new EntityCollection();
                // Initialize query
                var query = new QueryExpression(entityName)
                {
                    ColumnSet = columnSet,
                    Criteria = new FilterExpression
                    {
                        Conditions =
         {
             new ConditionExpression(ContactPropertyTableColumnNames.Property, ConditionOperator.Equal, propertyId)
         }
                    },
                    PageInfo = new PagingInfo
                    {
                        Count = 5000,
                        PageNumber = 1
                    }
                };
                do
                {
                    var currentPage = service.RetrieveMultiple(query);
                    allRecords.Entities.AddRange(currentPage.Entities);
                    if (currentPage.MoreRecords)
                    {
                        query.PageInfo.PageNumber++;
                        query.PageInfo.PagingCookie = currentPage.PagingCookie;
                    }
                    else
                    {
                        break;
                    }
                } while (true);

                tracingService.Trace("{0}", "Exiting from 'RetrievePropertiesContact' method after execution.");
                return allRecords;
            }
            catch (Exception ex)
            {
                tracingService.Trace("An exception occurred while executing RetrievePropertiesContact: {0}.", ex.ToString());
                throw new InvalidPluginExecutionException("An error occurred while retrieve  properties contact: " + ex.Message + ".");
            }
        }


    }


}

