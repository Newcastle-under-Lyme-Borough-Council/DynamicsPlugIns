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
using System.Runtime.Remoting.Contexts;
using System.Diagnostics.Contracts;
using System.ServiceModel.Channels;

namespace SS.MSDYN.LGIntelliware.Plugins
{
    /// <summary>
    /// Helper class to provide a set of common functions.
    /// </summary>
    public static class DataverseHelper
    {
        //Retrieves an active service configuration record by id
        public static EntityCollection RetrieveServiceConfiguration(this IOrganizationService service, string entityName, Guid serviceConfigurationId, ColumnSet columnSet)
        {
            try
            {
                //Query to fetch service configuration record
                QueryExpression query = new QueryExpression(entityName)
                {
                    ColumnSet = columnSet,
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                         new ConditionExpression(ServiceConfiguration.ServiceConfigurationid, ConditionOperator.Equal, serviceConfigurationId),
                         new ConditionExpression(ServiceConfiguration.Status, ConditionOperator.Equal, StateCode.Active.GetHashCode()) ,
                         new ConditionExpression(ServiceConfiguration.StatusReason, ConditionOperator.Equal, StatusCode.Active.GetHashCode())
                        }
                    }
                };
                //Execute query and return result
                return service.RetrieveMultiple(query);
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("Fault exception occured executing RetrieveServiceConfiguration: " + ex.Message + ".");
            }
        }

        // Retrieves a subject record by its title.
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
                         new ConditionExpression(Subject.Title, ConditionOperator.Equal, title)
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

        //Create a new incident (case) in dataverse
        public static Guid CreateIncident(this IOrganizationService service, Entity entity, Entity serviceConfiguration, Entity subject)
        {
            try
            {
                var entityToCreate = new Entity(Incident.TableName);
                // For each  attribute check if it exists and then set it on the new Incident record.
                if (serviceConfiguration.Attributes.ContainsKey(ServiceConfiguration.ServiceConfigurationid) && (Guid) serviceConfiguration.Attributes[ServiceConfiguration.ServiceConfigurationid] != Guid.Empty)
                {
                    entityToCreate.Attributes.Add(Incident.Service, new EntityReference(ServiceConfiguration.TableName, serviceConfiguration.GetAttributeValue<Guid>(ServiceConfiguration.ServiceConfigurationid)));
                }
                if ((serviceConfiguration.Attributes.ContainsKey(ServiceConfiguration.ServiceConfigurationid) && (Guid)serviceConfiguration.Attributes[ServiceConfiguration.ServiceConfigurationid] != Guid.Empty) && (entity.Attributes.ContainsKey(ServiceRequest.ReferenceNumber) && entity.Attributes[ServiceRequest.ReferenceNumber] != null))
                {
                    entityToCreate.Attributes.Add(Incident.CaseTitle, entity.Attributes[ServiceRequest.ReferenceNumber] + " - " + serviceConfiguration.Attributes[ServiceConfiguration.Name] + " - " + " Service Request ");
                }
                if ((serviceConfiguration.Attributes.ContainsKey(ServiceConfiguration.ServiceConfigurationid) && (Guid)serviceConfiguration.Attributes[ServiceConfiguration.ServiceConfigurationid] != Guid.Empty) && (entity.Attributes.ContainsKey(ServiceRequest.GardenWasteReferenceNumber) && entity.Attributes[ServiceRequest.GardenWasteReferenceNumber] != null))
                {
                    entityToCreate.Attributes.Add(Incident.CaseTitle, entity.Attributes[ServiceRequest.GardenWasteReferenceNumber] + " - " + serviceConfiguration.Attributes[ServiceConfiguration.Name] + " - " + " Service Request ");
                }

                entityToCreate.Attributes.Add(Incident.CaseType, new OptionSetValue(IncidentType.Request.GetHashCode()));
                if (entity.Attributes.ContainsKey(ServiceRequest.Customer) && ((EntityReference)entity.Attributes[ServiceRequest.Customer]).Id != Guid.Empty)
                {
                    entityToCreate.Attributes.Add(Incident.Customer, new EntityReference(Contact.TableName, entity.GetAttributeValue<EntityReference>(ServiceRequest.Customer).Id));
                }

                if (entity.Attributes.ContainsKey(ServiceRequest.GardenWasteContact) && ((EntityReference)entity.Attributes[ServiceRequest.GardenWasteContact]).Id != Guid.Empty)
                {
                    entityToCreate.Attributes.Add(Incident.Customer, new EntityReference(Contact.TableName, entity.GetAttributeValue<EntityReference>(ServiceRequest.GardenWasteContact).Id));
                }

                if (entity.Attributes.ContainsKey(ServiceRequest.Description) && entity.Attributes[ServiceRequest.Description] != null)
                {
                    entityToCreate.Attributes.Add(Incident.Description, entity.GetAttributeValue<string>(ServiceRequest.Description));
                }

                if (entity.Attributes.ContainsKey(ServiceRequest.Owner) && ((EntityReference) entity.Attributes[ServiceRequest.Owner]).Id != Guid.Empty)
                {
                    entityToCreate.Attributes.Add(Incident.Owner, new EntityReference(SystemUser.TableName, entity.GetAttributeValue<EntityReference>(ServiceRequest.Owner).Id));
                }

                entityToCreate.Attributes.Add(Incident.Priority, new OptionSetValue(IncidentPriority.Normal.GetHashCode()));

                if (entity.Attributes.ContainsKey(ServiceRequest.ReportedBy) && ((EntityReference)entity.Attributes[ServiceRequest.ReportedBy]).Id != Guid.Empty)
                {
                    entityToCreate.Attributes.Add(Incident.ReportedBy, new EntityReference(Contact.TableName, entity.GetAttributeValue<EntityReference>(ServiceRequest.ReportedBy).Id));
                }

                entityToCreate.Attributes.Add(Incident.ServiceRequest, new EntityReference(entity.LogicalName, entity.Id));

                if (subject.Attributes.ContainsKey(Subject.SubjectId) && ((Guid) subject.Attributes[Subject.SubjectId]) !=Guid.Empty)
                {
                    entityToCreate.Attributes.Add(Incident.Subject, new EntityReference(Subject.TableName, subject.GetAttributeValue<Guid>(Subject.SubjectId)));
                }

                if (entity.Attributes.ContainsKey(ServiceRequest.ReferenceNumber) && entity.Attributes[ServiceRequest.ReferenceNumber] != null)
                {
                    entityToCreate.Attributes.Add(Incident.CaseNumber, entity.GetAttributeValue<string>(ServiceRequest.ReferenceNumber));
                }

                if (entity.Attributes.ContainsKey(ServiceRequest.GardenWasteReferenceNumber) && entity.Attributes[ServiceRequest.GardenWasteReferenceNumber] != null)
                {
                    entityToCreate.Attributes.Add(Incident.CaseNumber, entity.GetAttributeValue<string>(ServiceRequest.GardenWasteReferenceNumber));
                }

                if (entity.Attributes.ContainsKey(ServiceRequest.SourceType) && entity.Attributes[ServiceRequest.SourceType] != null)
                {
                    var missedBinSourceType = entity.GetAttributeValue<OptionSetValue>(ServiceRequest.SourceType).Value;
                    if (missedBinSourceType == ServiceRequestSourceType.Phone.GetHashCode())
                        entityToCreate.Attributes.Add(Incident.Origin, new OptionSetValue(IncidentOrigin.Phone.GetHashCode()));
                    else if (missedBinSourceType == ServiceRequestSourceType.Web.GetHashCode())
                        entityToCreate.Attributes.Add(Incident.Origin, new OptionSetValue(IncidentOrigin.Web.GetHashCode()));
                    else if (missedBinSourceType == ServiceRequestSourceType.Email.GetHashCode())
                        entityToCreate.Attributes.Add(Incident.Origin, new OptionSetValue(IncidentOrigin.Email.GetHashCode()));
                    else if (missedBinSourceType == ServiceRequestSourceType.Portal.GetHashCode())
                        entityToCreate.Attributes.Add(Incident.Origin, new OptionSetValue(IncidentOrigin.Portal.GetHashCode()));
                }

                if (entity.Attributes.ContainsKey(ServiceRequest.ContactProperty) && ((EntityReference)entity.Attributes[ServiceRequest.ContactProperty]).Id!= Guid.Empty)
                {
                    entityToCreate.Attributes.Add(Incident.ContactProperty, new EntityReference(ContactProperty.TableName, entity.GetAttributeValue<EntityReference>(ServiceRequest.ContactProperty).Id));
                }
                if (entity.Attributes.ContainsKey(ServiceRequest.Property) && entity.Attributes[ServiceRequest.Property] != null)
                {

                    entityToCreate.Attributes.Add(Incident.Property, entity.GetAttributeValue<string>(ServiceRequest.Property));
                }

                if (entity.Attributes.ContainsKey(ServiceRequest.PropertyUprn) && ((EntityReference)entity.Attributes[ServiceRequest.PropertyUprn]).Id != Guid.Empty)
                {
                    entityToCreate.Attributes.Add(Incident.PropertyUprn, new EntityReference(Property.TableName, entity.GetAttributeValue<EntityReference>(ServiceRequest.PropertyUprn).Id));
                }
                // Check if the current entity does not already contain a contact property and if the entity is of type missedbin
                if (!entity.Contains(ServiceRequest.ContactProperty) && entity.LogicalName.Equals(ServiceRequest.MissedBinTableName))
                {
                    // Retrieve the default contactProperty record for the related Customer
                    var contactProperty = RetrieveDefaultContactProperty(service, ContactProperty.TableName, entity.GetAttributeValue<EntityReference>(ServiceRequest.Customer).Id, new ColumnSet(ContactProperty.Property, ContactProperty.ContactPropertyId));
                    if (contactProperty != null)
                    {
                        var propertyId = contactProperty.GetAttributeValue<EntityReference>(ContactProperty.Property).Id;
                        var contactPropertyId = contactProperty.GetAttributeValue<Guid>(ContactProperty.ContactPropertyId);
                        if (propertyId != Guid.Empty)
                        {
                            var property = RetrieveProperty(service, Property.TableName, propertyId, new ColumnSet(Property.Addresscs));
                            // If the property record is found, set its address on the Incident record being created
                            if (property != null)
                            {
                                entityToCreate.Attributes.Add(Incident.Property, property.GetAttributeValue<string>(Property.Addresscs));
                                entityToCreate.Attributes.Add(Incident.PropertyUprn, new EntityReference(Property.TableName, propertyId));

                            }
                        }

                        entityToCreate.Attributes.Add(Incident.ContactProperty, new EntityReference(ContactProperty.TableName, contactPropertyId));
                    }

                }
                return service.Create(entityToCreate); ;
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("Fault exception occured executing CreateIncident: " + ex.Message + ".");
            }
        }

        //Updates the specified entity in Dataverse.
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

        //Retrieve a contact record
        public static Entity RetrieveContact(this IOrganizationService service, string entityName, Guid contactId, ColumnSet columnSet)
        {
            try
            {
                return service.Retrieve(entityName, contactId, columnSet);
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An error occurred while retrieve contact: " + ex.Message + ".");
            }
        }


        //Retrieve contact property by its id
        public static Entity RetrieveContactProperty(this IOrganizationService service, string entityName, Guid contactPropertyId, ColumnSet columnSet)
        {
            try
            {
                return service.Retrieve(entityName, contactPropertyId, columnSet);

            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An error occurred while retrieve contact property: " + ex.Message + ".");
            }
        }

        //Retrieve contact properties 
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
                    new ConditionExpression(ContactProperty.Contact, ConditionOperator.Equal, contactId)
                }
                    },
                    PageInfo = new PagingInfo
                    {
                        Count = 5000,
                        PageNumber = 1
                    }
                };
                // Create entity collection instance to store all record
                var allRecords = new EntityCollection();
                //Loop through all pages until no more records
                do
                {
                    var currentPage = service.RetrieveMultiple(query);
                    allRecords.Entities.AddRange(currentPage.Entities);
                    // If there are more records, update paging info and continue
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
                // Return the complete collection of contact properties
                return allRecords;
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An error occurred while retrieve contact properties: " + ex.Message + ".");
            }
        }

        //Retrieve property by its id
        public static Entity RetrieveProperty(this IOrganizationService service, string entityName, Guid propertyId, ColumnSet columnSet)
        {
            try
            {
                return service.Retrieve(entityName, propertyId, columnSet);

            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An error occurred while retrieve property: " + ex.Message + ".");
            }
        }

        //Create property record 
        public static Guid CreateProperty(this IOrganizationService service, Entity contact, string uprn)
        {
            try
            {
                var entityToCreate = new Entity(Property.TableName);
                entityToCreate[Property.Uprn] = uprn;
                entityToCreate[Property.Addresscs] = contact.GetAttributeValue<string>(Contact.Address1_address);
                entityToCreate[Property.County] = contact.GetAttributeValue<string>(Contact.County);
                entityToCreate[Property.Addressoscs] = contact.GetAttributeValue<string>(Contact.Address);
                entityToCreate[Property.Localityname] = contact.GetAttributeValue<string>(Contact.Address2);
                entityToCreate[Property.Streetname] = contact.GetAttributeValue<string>(Contact.Address1_line3);
                entityToCreate[Property.TownName] = contact.GetAttributeValue<string>(Contact.City);
                entityToCreate[Property.PostCode] = contact.GetAttributeValue<string>(Contact.PostCode);
                entityToCreate[Property.Posttown] = contact.GetAttributeValue<string>(Contact.Stateorprovince);
                entityToCreate[Property.Region] = contact.GetAttributeValue<string>(Contact.Country);
                entityToCreate[Property.Latitude] = contact.GetAttributeValue<double>(Contact.Latitude);
                entityToCreate[Property.Longitude] = contact.GetAttributeValue<double>(Contact.Longitude);

                return service.Create(entityToCreate);
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An error occurred while creating records in table property: " + ex.Message + ".");
            }
        }

        //Updates a property record using address details from a Contact record
        public static Guid UpdatePropertyFromContact(this IOrganizationService service, Entity contact, string uprn, Guid propertyId)
        {
            try
            {
                var entityToUpdate = new Entity(Property.TableName)
                {
                    Id = propertyId
                };
                entityToUpdate[Property.Addresscs] = contact.GetAttributeValue<string>(Contact.Address1_address);
                entityToUpdate[Property.County] = contact.GetAttributeValue<string>(Contact.County);
                entityToUpdate[Property.Addressoscs] = contact.GetAttributeValue<string>(Contact.Address);
                entityToUpdate[Property.Localityname] = contact.GetAttributeValue<string>(Contact.Address2);
                entityToUpdate[Property.Streetname] = contact.GetAttributeValue<string>(Contact.Address1_line3);
                entityToUpdate[Property.TownName] = contact.GetAttributeValue<string>(Contact.City);
                entityToUpdate[Property.PostCode] = contact.GetAttributeValue<string>(Contact.PostCode);
                entityToUpdate[Property.Posttown] = contact.GetAttributeValue<string>(Contact.Stateorprovince);
                entityToUpdate[Property.Region] = contact.GetAttributeValue<string>(Contact.Country);
                entityToUpdate[Property.Latitude] = contact.GetAttributeValue<double?>(Contact.Latitude);
                entityToUpdate[Property.Longitude] = contact.GetAttributeValue<double?>(Contact.Longitude);
                service.Update(entityToUpdate);

                return propertyId;
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An error occurred while updating records in table ` property`: " + ex.Message + ".");
            }
        }
        //Updates a property record using new values from the same Property record
        public static Guid UpdatePropertyDetails(this IOrganizationService service, Entity property, string uprn, Guid propertyId)
        {
            try
            {
                var entityToUpdate = new Entity(Property.TableName)
                {
                    Id = propertyId
                };
                entityToUpdate[Property.Addresscs] = property.GetAttributeValue<string>(Property.Addresscs);
                entityToUpdate[Property.County] = property.GetAttributeValue<string>(Property.County);
                entityToUpdate[Property.Addressoscs] = property.GetAttributeValue<string>(Property.Addressoscs);
                entityToUpdate[Property.Localityname] = property.GetAttributeValue<string>(Property.Localityname);
                entityToUpdate[Property.Streetname] = property.GetAttributeValue<string>(Property.Streetname);
                entityToUpdate[Property.TownName] = property.GetAttributeValue<string>(Property.TownName);
                entityToUpdate[Property.PostCode] = property.GetAttributeValue<string>(Property.PostCode);
                entityToUpdate[Property.Posttown] = property.GetAttributeValue<string>(Property.Posttown);
                entityToUpdate[Property.Region] = property.GetAttributeValue<string>(Property.Region);
                entityToUpdate[Property.Latitude] = property.GetAttributeValue<double?>(Property.Latitude);
                entityToUpdate[Property.Longitude] = property.GetAttributeValue<double?>(Property.Longitude);
                service.Update(entityToUpdate);

                return propertyId;
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An error occurred while updating records in table ` property`: " + ex.Message + ".");
            }
        }

        // Updates a contact record with property details from a given property entity.
        public static void UpdateContactwithPropertyData(IOrganizationService service, string tableName, Guid Contactid, Entity property)
        {
            var entityToCreate = new Entity(Contact.TableName, Contactid);
            entityToCreate[Contact.Uprn] = property.GetAttributeValue<string>(Property.Uprn);
            entityToCreate[Contact.Address1_address] = property.GetAttributeValue<string>(Property.Addresscs);
            entityToCreate[Contact.County] = property.GetAttributeValue<string>(Property.County);
            entityToCreate[Contact.Address] = property.GetAttributeValue<string>(Property.Addressoscs);
            entityToCreate[Contact.Address2] = property.GetAttributeValue<string>(Property.Localityname);
            entityToCreate[Contact.Address1_line3] = property.GetAttributeValue<string>(Property.Streetname);
            entityToCreate[Contact.City] = property.GetAttributeValue<string>(Property.TownName);
            entityToCreate[Contact.PostCode] = property.GetAttributeValue<string>(Property.PostCode);
            entityToCreate[Contact.Stateorprovince] = property.GetAttributeValue<string>(Property.Posttown);
            entityToCreate[Contact.Country] = property.GetAttributeValue<string>(Property.Region);
            entityToCreate[Contact.Latitude] = property.GetAttributeValue<double>(Property.Latitude);
            entityToCreate[Contact.Longitude] = property.GetAttributeValue<double>(Property.Longitude);

            service.Update(entityToCreate);
        }


        //Remove the default flag from a specific contact property record.
        public static void RemoveOtherContactPropertyfromDefault(this IOrganizationService service, Guid contactPropertyId)
        {
            try
            {
                var entityToUpdate = new Entity(ContactProperty.TableName)
                {
                    Id = contactPropertyId
                };

                entityToUpdate[ContactProperty.IsDefault] = false;
                service.Update(entityToUpdate);
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An error occurred while removing contact property from default : " + ex.Message, ex);
            }
        }

        //Set contact property as default
        public static void SetContactPropertyToDefault(this IOrganizationService service, Guid contactPropertyId)
        {
            try
            {
                var entityToUpdate = new Entity(ContactProperty.TableName)
                {
                    Id = contactPropertyId
                };

                entityToUpdate[ContactProperty.IsDefault] = true;
                service.Update(entityToUpdate);
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An error occurred while updating the contact property record: " + ex.Message, ex);
            }
        }

        // Creates a new contact property relationship record in dataverse, linking a contact to a property.
        public static Guid CreatePropertyContact(this IOrganizationService service, Guid contact, Guid propertyId, bool IsDefault)
        {
            try
            {
                var entityToCreate = new Entity(ContactProperty.TableName);
                entityToCreate[ContactProperty.Contact] = new EntityReference(Contact.TableName, contact);
                entityToCreate[ContactProperty.Property] = new EntityReference(Property.TableName, propertyId);
                entityToCreate[ContactProperty.IsDefault] = IsDefault;
                return service.Create(entityToCreate);
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An error occurred while creating records in table contact property: " + ex.Message + ".");
            }
        }

        // Checks whether a property record exists for a given UPRN.
        public static Guid CheckPropertiesExist(this IOrganizationService service, string entityName, string uprn, ColumnSet columnSet)
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
                    new ConditionExpression(Property.Uprn, ConditionOperator.Equal, uprn)
                }
                    },
                    Orders =
            {
                new OrderExpression(Property.CreatedOn, OrderType.Ascending) // oldest first
            },
                    TopCount = 1 // only fetch the top 1 result
                };

                var records = service.RetrieveMultiple(query);
                if (records.Entities.Count > 0)
                {
                    return records.Entities[0].Id;
                }

                return Guid.Empty;
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException($"Error checking properties: {ex.Message}", ex);
            }
        }

        //Delete property by its id
        public static void DeleteProperty(this IOrganizationService service, string entityName, Guid propertyId)
        {
            try
            {
                service.Delete(entityName, propertyId);
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("Fault exception occurred executing DeleteProperty: " + ex.Message + ".", ex);
            }
        }

        // Retrieves the default contact property record for a given contact.
        public static Entity RetrieveDefaultContactProperty(this IOrganizationService service, string entityName, Guid contactId, ColumnSet columnSet)
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
                         new ConditionExpression(ContactProperty.Contact, ConditionOperator.Equal, contactId),
                         new ConditionExpression(ContactProperty.IsDefault, ConditionOperator.Equal, true) ,
                        }
                    }
                };
                var result = service.RetrieveMultiple(query);
                return result.Entities.Count > 0 ? result.Entities[0] : null;
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("Fault exception occured executing RetrieveDefaultContactProperty: " + ex.Message + ".");
            }
        }
    }
}

