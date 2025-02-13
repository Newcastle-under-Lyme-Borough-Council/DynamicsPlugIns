using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.Messages;
using System.Collections;
using System.Runtime.Remoting.Services;
using System.Web.UI.WebControls;
using System.IdentityModel.Metadata;

namespace SS.MSDYN.LGIntelliware.Plugins
{
    /// <summary>
    /// Helper class to provide a set of common functions.
    /// </summary>
    public static class DataverseHelper
    {
        public static EntityCollection RetrieveServiceConfiguration(this IOrganizationService service, string entityName, Guid serviceConfigurationId, ColumnSet columnSet)
        {
            try
            {
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
                return service.RetrieveMultiple(query);
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("Fault exception occured executing RetrieveServiceConfiguration: " + ex.Message + ".");
            }
        }
        public static EntityCollection RetrieveSubject(this IOrganizationService service, string entityName, string title, ColumnSet columnSet)
        {
            try
            {
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
                return service.RetrieveMultiple(query);
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("Fault exception occured executing RetrieveSubject: " + ex.Message + ".");
            }
        }
        public static Guid CreateIncident(this IOrganizationService service, Entity entity, Entity serviceConfiguration, Entity subject)
        {
            try
            {
                var entityToCreate = new Entity(IncidentTableColumnNames.TableName);
                if (serviceConfiguration.Attributes.ContainsKey(ServiceConfigurationTableColumnNames.ServiceConfiguration) && serviceConfiguration.Attributes[ServiceConfigurationTableColumnNames.ServiceConfiguration] != null)
                {
                    entityToCreate.Attributes.Add(IncidentTableColumnNames.Service, new EntityReference(ServiceConfigurationTableColumnNames.TableName, serviceConfiguration.GetAttributeValue<Guid>(ServiceConfigurationTableColumnNames.ServiceConfiguration)));
                }

                if ((serviceConfiguration.Attributes.ContainsKey(ServiceConfigurationTableColumnNames.ServiceConfiguration) && serviceConfiguration.Attributes[ServiceConfigurationTableColumnNames.ServiceConfiguration] != null) && (entity.Attributes.ContainsKey(ServiceRequestTableColumnNames.ReferenceNumber) && entity.Attributes[ServiceRequestTableColumnNames.ReferenceNumber] != null))
                {
                    entityToCreate.Attributes.Add(IncidentTableColumnNames.CaseTitle, entity.Attributes[ServiceRequestTableColumnNames.ReferenceNumber] + " - " + serviceConfiguration.Attributes[ServiceConfigurationTableColumnNames.Name] + " - " + " Service Request ");
                }

                entityToCreate.Attributes.Add(IncidentTableColumnNames.CaseType, new OptionSetValue(IncidentType.Request.GetHashCode()));

                if (entity.Attributes.ContainsKey(ServiceRequestTableColumnNames.Customer) && entity.Attributes[ServiceRequestTableColumnNames.Customer] != null)
                {
                    entityToCreate.Attributes.Add(IncidentTableColumnNames.Customer, new EntityReference(ContactTableColumnNames.TableName, entity.GetAttributeValue<EntityReference>(ServiceRequestTableColumnNames.Customer).Id));
                }

                if (entity.Attributes.ContainsKey(ServiceRequestTableColumnNames.Description) && entity.Attributes[ServiceRequestTableColumnNames.Description] != null)
                {
                    entityToCreate.Attributes.Add(IncidentTableColumnNames.Description, entity.GetAttributeValue<string>(ServiceRequestTableColumnNames.Description));
                }

                if (entity.Attributes.ContainsKey(ServiceRequestTableColumnNames.Owner) && entity.Attributes[ServiceRequestTableColumnNames.Owner] != null)
                {
                    entityToCreate.Attributes.Add(IncidentTableColumnNames.Owner, new EntityReference(SystemUserTableColumnNames.TableName, entity.GetAttributeValue<EntityReference>(ServiceRequestTableColumnNames.Owner).Id));
                }

                entityToCreate.Attributes.Add(IncidentTableColumnNames.Priority, new OptionSetValue(IncidentPriority.Normal.GetHashCode()));

                if (entity.Attributes.ContainsKey(ServiceRequestTableColumnNames.ReportedBy) && entity.Attributes[ServiceRequestTableColumnNames.ReportedBy] != null)
                {
                    entityToCreate.Attributes.Add(IncidentTableColumnNames.ReportedBy, new EntityReference(ContactTableColumnNames.TableName, entity.GetAttributeValue<EntityReference>(ServiceRequestTableColumnNames.ReportedBy).Id));
                }

                entityToCreate.Attributes.Add(IncidentTableColumnNames.ServiceRequest, new EntityReference(entity.LogicalName, entity.Id));


                if (subject.Attributes.ContainsKey(SubjectTableColumnNames.Subject) && subject.Attributes[SubjectTableColumnNames.Subject] != null)
                {
                    entityToCreate.Attributes.Add(IncidentTableColumnNames.Subject, new EntityReference(SubjectTableColumnNames.TableName, subject.GetAttributeValue<Guid>(SubjectTableColumnNames.Subject)));
                }

                if (entity.Attributes.ContainsKey(ServiceRequestTableColumnNames.ReferenceNumber) && entity.Attributes[ServiceRequestTableColumnNames.ReferenceNumber] != null)
                {
                    entityToCreate.Attributes.Add(IncidentTableColumnNames.CaseNumber, entity.GetAttributeValue<string>(ServiceRequestTableColumnNames.ReferenceNumber));
                }

                if (entity.Attributes.ContainsKey(ServiceRequestTableColumnNames.SourceType) && entity.Attributes[ServiceRequestTableColumnNames.SourceType] != null)
                {
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
                return service.Create(entityToCreate); ;
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("Fault exception occured executing CreateIncident: " + ex.Message + ".");
            }
        }
        public static void Update(this IOrganizationService service, Entity entityToUpdate)
        {
            try
            {
                service.Update(entityToUpdate);
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("Fault exception occured executing Update: " + ex.Message + ".");
            }
        }

        public static bool RetrieveCreatedProperty(this IOrganizationService service, string entityName, string uprn, Guid contactId, ColumnSet columnSet)
        {
            try
            {
                if (uprn != null)
                {


                    var query = new QueryExpression(entityName)
                    {
                        ColumnSet = columnSet,
                        Criteria = new FilterExpression
                        {
                            Conditions =
                {
                    new ConditionExpression(PropertyTableColumnNames.Uprn, ConditionOperator.Equal, uprn),
                    new ConditionExpression(PropertyTableColumnNames.Contact, ConditionOperator.Equal, contactId),
                }
                        }
                    };

                    var records = service.RetrieveMultiple(query);
                    if (records.Entities.Count > 0)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An error occurred while retrieving properties: " + ex.Message, ex);
            }
        }


        public static void CreateContactProperty(this IOrganizationService service, EntityCollection properties, Guid contactId)
        {
            try
            {
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
                            //   tracingService.Trace("Error creating record in table `contactproperty`: {0}", responseItem.Fault.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An error occurred while creating records in table `contact property`: " + ex.Message + ".");
            }
        }
        public static EntityCollection RetrieveContacts(this IOrganizationService service, string entityName, string uprn, ColumnSet columnSet)
        {
            try
            {
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

                return allRecords;
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An error occurred while retrieving contacts: " + ex.Message + ".");
            }
        }
        public static void AddContactProperty(this IOrganizationService service, EntityCollection contacts, Guid propertyId)
        {
            try
            {
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
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An error occurred while creating records in table `contact property`: " + ex.Message + ".");
            }
        }
        public static Entity RetrieveContactProperty(this IOrganizationService service, string entityName, Guid contactPropertyId, ColumnSet columnSet)
        {
            try
            {
                return service.Retrieve(entityName, contactPropertyId, columnSet);

            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An error occurred while retrieve contact properties: " + ex.Message + ".");
            }
        }

        public static EntityCollection RetrieveContactProperties(this IOrganizationService service, string entityName, Guid contactId, ColumnSet columnSet)
        {
            try
            {
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

                return allRecords;
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An error occurred while retrieve contact properties: " + ex.Message + ".");
            }
        }
        public static void DeleteContactProperties(this IOrganizationService service, EntityCollection contactProperties)
        {
            try
            {
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
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An error occurred while deleting records in table `contact property`: " + ex.Message + ".");
            }
        }
        public static EntityCollection RetrievePropertiesContact(this IOrganizationService service, string entityName, Guid propertyId, ColumnSet columnSet)
        {
            try
            {
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

                return allRecords;
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An error occurred while retrieve  properties contact: " + ex.Message + ".");
            }
        }

        public static void AddCreatedProperty(this IOrganizationService service, string contact, Guid propertyId)
        {
            try
            {
                var entityToCreate = new Entity(ContactPropertyTableColumnNames.TableName)
                {
                    [ContactPropertyTableColumnNames.Property] = new EntityReference(PropertyTableColumnNames.TableName, propertyId),
                    [ContactPropertyTableColumnNames.Contact] = new EntityReference(ContactTableColumnNames.TableName, new Guid(contact))
                };
                service.Create(entityToCreate);

            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An error occurred while creating records in table `contact property`: " + ex.Message + ".");
            }
        }

        
        public static void AddCreatedContact(this IOrganizationService service, string contact, Guid propertyId)
        {
            try
            {
                var entityToCreate = new Entity(ContactPropertyTableColumnNames.TableName)
                {
                    [ContactPropertyTableColumnNames.Property] = new EntityReference(PropertyTableColumnNames.TableName, propertyId),
                    [ContactPropertyTableColumnNames.Contact] = new EntityReference(ContactTableColumnNames.TableName, new Guid(contact))
                };
                service.Create(entityToCreate);

            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An error occurred while creating records in table `contact property`: " + ex.Message + ".");
            }
        }

        public static void AddUpdatedProperty(this IOrganizationService service, string contact, Guid propertyId)
        {
            try
            {
                var entityToUpdate = new Entity(ContactPropertyTableColumnNames.TableName)
                {
                    [ContactPropertyTableColumnNames.Property] = new EntityReference(PropertyTableColumnNames.TableName, propertyId)
                };
                service.Update(entityToUpdate);

            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An error occurred while creating records in table `contact property`: " + ex.Message + ".");
            }
        }

        public static Guid RetrieveProperty(this IOrganizationService service, string entityName, string contact, Guid propertyId)
        {
            try
            {
                var columnSet = new ColumnSet(false);

                var query = new QueryExpression(entityName)
                {
                    ColumnSet = columnSet,
                    Criteria = new FilterExpression
                    {
                        Conditions =
                {
                    new ConditionExpression(ContactPropertyTableColumnNames.Contact, ConditionOperator.Equal, new Guid(contact)),
                    new ConditionExpression(ContactPropertyTableColumnNames.Property, ConditionOperator.Equal, propertyId)
                }
                    }
                };

                var entityCollection = service.RetrieveMultiple(query);

                if (entityCollection.Entities.Count > 0)
                {
                    return entityCollection.Entities[0].Id;
                }

                else
                {
                    return new Guid();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An error occurred while retrieving records from table `contact property`: " + ex.Message, ex);
            }
        }

        public static Guid CreateProperty(this IOrganizationService service, Guid contactId, Entity contact, string uprn)
        {
            try
            {
                var entityToCreate = new Entity(PropertyTableColumnNames.TableName);
                entityToCreate[PropertyTableColumnNames.Uprn] = uprn;
                entityToCreate[PropertyTableColumnNames.PostCode] = contact.GetAttributeValue<string>(ContactTableColumnNames.PostCode);
                entityToCreate[PropertyTableColumnNames.Address] = contact.GetAttributeValue<string>(ContactTableColumnNames.Address);
                entityToCreate[PropertyTableColumnNames.Contact] = contactId.ToString();
                return service.Create(entityToCreate);
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An error occurred while creating records in table `contact property`: " + ex.Message + ".");
            }

        }


        public static void RemoveOtherContactPropertyfromDefault(this IOrganizationService service, Guid contactPropertyId)
        {
            try
            {
                var entityToUpdate = new Entity(ContactPropertyTableColumnNames.TableName)
                {
                    Id = contactPropertyId
                };

                entityToUpdate[ContactPropertyTableColumnNames.IsDefault] = false;
                service.Update(entityToUpdate);
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An error occurred while updating the property record: " + ex.Message, ex);
            }
        }
        public static void SetPropertyToDefault(this IOrganizationService service, Guid contactPropertyId)
        {
            try
            {
                var entityToUpdate = new Entity(ContactPropertyTableColumnNames.TableName)
                {
                    Id = contactPropertyId
                };

                entityToUpdate[ContactPropertyTableColumnNames.IsDefault] = true;
                service.Update(entityToUpdate);
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An error occurred while updating the contact property record: " + ex.Message, ex);
            }
        }

        public static Guid CreatePropertyContact(this IOrganizationService service, Guid contact, Guid propertyId)
        {
            try
            {
                var entityToCreate = new Entity(ContactPropertyTableColumnNames.TableName);
                entityToCreate[ContactPropertyTableColumnNames.Contact] = new EntityReference(ContactTableColumnNames.TableName, contact);
                entityToCreate[ContactPropertyTableColumnNames.Property] = new EntityReference(PropertyTableColumnNames.TableName, propertyId);
                return service.Create(entityToCreate);
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An error occurred while creating records in table `contact property`: " + ex.Message + ".");
            }
        }

        public static EntityCollection RetrievePropertiesContact(this IOrganizationService service, string entityName, Guid contactId, string uprn, ColumnSet columnSet)
        {
            try
            {
                var query = new QueryExpression(entityName)
                {
                    ColumnSet = columnSet,
                    Criteria = new FilterExpression
                    {
                        Conditions =
                {
                    new ConditionExpression(PropertyTableColumnNames.Contact, ConditionOperator.Equal, contactId.ToString()),
                    //new ConditionExpression(PropertyTableColumnNames.Uprn, ConditionOperator.Equal, uprn)
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

                return allRecords;
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An error occurred while retrieve contact properties: " + ex.Message + ".");
            }
        }

        public static Guid CheckPropertiesContactExist(this IOrganizationService service, string entityName, Guid contactId, string uprn, ColumnSet columnSet)
        {
            try
            {
                var query = new QueryExpression(entityName)
                {
                    ColumnSet = columnSet,
                    Criteria = new FilterExpression
                    {
                        Conditions =
                {
                    new ConditionExpression(PropertyTableColumnNames.Contact, ConditionOperator.Equal, contactId.ToString()),
                    new ConditionExpression(PropertyTableColumnNames.Uprn, ConditionOperator.Equal, uprn)
                }
                    },

                };

                var records = service.RetrieveMultiple(query);
                if (records.Entities.Count > 0)
                {
                    return records.Entities[0].Id;
                }

                return new Guid();
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An error occurred while retrieve property: " + ex.Message + ".");
            }
        }


    }
}

