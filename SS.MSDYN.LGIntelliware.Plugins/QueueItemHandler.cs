using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using SS.MSDYN.LGIntelliware.Plugins.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Services;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SS.MSDYN.LGIntelliware.Plugins
{
    public class QueueItemHandler : PluginBase
    {
        protected PluginConfig Config = new PluginConfig();

        //Registers the plugin to run after a entity record is created or updated.
        public QueueItemHandler() : base(typeof(QueueItemHandler))
        {
            RegisterEvents();
        }

        public void RegisterEvents()
        {
            RegisteredEvents.Add(new Tuple<int, string, string, Action<LocalPluginContext>>(PluginExecutionPipelineStage.PostOperation.GetHashCode(), PluginExecutionMessageName.UPDATE, "ss_incident", Execute));
            RegisteredEvents.Add(new Tuple<int, string, string, Action<LocalPluginContext>>(PluginExecutionPipelineStage.PostOperation.GetHashCode(), PluginExecutionMessageName.UPDATE, "queueitem", Execute));
        }
        protected void Execute(LocalPluginContext localContext)
        {

            if (localContext == null)
            {
                throw new ArgumentNullException(nameof(localContext));
            }
            var context = localContext.PluginExecutionContext;
            var service = localContext.OrganizationService;
            var tracingService = localContext.TracingService;

            // Prevent infinite loops by limiting depth
            if (context.Depth > 2)
            {
                return;
            }

            try
            {
                // Check if context message name is 'Create' ...
                if (!context.MessageName.Equals(PluginExecutionMessageName.UPDATE))
                {
                    throw new InvalidPluginExecutionException("Plugin execution expects an Update message.");
                }

                localContext.TracingService.Trace("message name is Update");

                // Check if context has 'Target' input parameter...
                if (context.InputParameters.ContainsKey(ContextInputParameters.TARGET) && context.InputParameters[ContextInputParameters.TARGET] == null)
                {
                    throw new InvalidPluginExecutionException("Target input parameter is missing or null");
                }

                localContext.TracingService.Trace("target is not null");

                var entity = (Entity)context.InputParameters[ContextInputParameters.TARGET];

                tracingService.Trace(entity.LogicalName);

                // Check if entity reference is of type Case summary...
                if (!entity.LogicalName.Equals(Case.TableName))
                {
                    throw new InvalidPluginExecutionException("Entity calling plugin is not Case summary entity.");
                }


                //Get record id of Case summary entity
                var recordId = entity.Id;

                if (recordId == Guid.Empty)
                {
                    throw new InvalidPluginExecutionException("Case summary record id is empty.");
                }

                //Get relevant column values of entity for initial checks - queueitem, queue and date submitted
                var currentRecord = service.Retrieve(
                    Case.TableName,
                    recordId,
                    new ColumnSet(
                        "nulbc_queueitem",
                        "nulbc_queue",
                        "nulbc_datesubmitted"
                    )
                );

                EntityReference queue = currentRecord.GetAttributeValue<EntityReference>("nulbc_queue");

                //DateTime is a value type and cannot be null
                DateTime? dateSubmitted = currentRecord.GetAttributeValue<DateTime?>("nulbc_datesubmitted").HasValue ? currentRecord.GetAttributeValue<DateTime?>("nulbc_datesubmitted") : null;

                if (!dateSubmitted.HasValue)
                {
                    tracingService.Trace("Incident has no date submitted");
                    return;
                }

                tracingService.Trace("Date submitted:" + dateSubmitted.Value);

                   
                //Check if the Case summary entity has a queue reference
                //if (currentRecord.GetAttributeValue<EntityReference>("nulbc_queue") == null && currentRecord.GetAttributeValue<EntityReference>("nulbc_queue").Id == Guid.Empty)
                //{
                //    tracingService.Trace("Incident type has no assigned queue");
                //    return;
                //}

                var queueItemRef = currentRecord.GetAttributeValue<EntityReference>("nulbc_queueitem");

                tracingService.Trace(
                    queueItemRef != null
                        ? $"Incident has a queue item reference: {queueItemRef.Id}"
                        : "Incident has no queue item reference");

                var queueItemQuery = DataverseHelper.RetrieveQueueItem(service, recordId);

                if (queueItemQuery != null)
                {
                    tracingService.Trace($"Incident has a queue item record: {queueItemQuery.Id}");
                }
                else
                {
                    tracingService.Trace("Incident has no queue item record");
                }



                //Check if the Case summary entity has no associated queue item reference
                if (queueItemRef == null && queueItemQuery == null)
                {

                    //Retrieve Unassigned User from User table
                    var unassignedUser = DataverseHelper.RetrieveUnassignedUser(service);

                    //var queueId = currentRecord.GetAttributeValue<EntityReference>("nulbc_queue").Id;
                    var queueId = Guid.Parse("c7c29624-c0a6-f111-aaad-7c1e522f31c2");

                    if (queueId != null)
                    {
                        try
                        {
                            var queueRetrieval = service.Retrieve(
                                "queue",
                                queueId,
                                new ColumnSet("name")
                                );
                        }
                        catch
                        {
                            throw new InvalidPluginExecutionException("Fault exception occured retrieving queue record.");
                        }

                        //check if queue reference is present in queue table
                        tracingService.Trace("Incident has a valid queue reference");

                        //retrieve the rest of the common attributes of the Case summary entity to create a queue item
                        Entity incidentAttributes = RetrieveCaseAttributes(service, recordId);

                        CreateQueueItem(service, tracingService, recordId, unassignedUser, queueId, incidentAttributes);

                        var newQueueItem = DataverseHelper.RetrieveQueueItem(service, recordId);

                        if (newQueueItem == null)
                        {
                            throw new InvalidPluginExecutionException("No new queue item reference");
                        }
                        tracingService.Trace("Queue item created successfully: " + newQueueItem.Id);

                        var EntityToUpdate = new Entity(Case.TableName)
                        {
                            Id = recordId
                        };


                        EntityToUpdate["nulbc_queueitem"] = new EntityReference(QueueItem.TableName, newQueueItem.Id);

                        service.Update(EntityToUpdate);

                        tracingService.Trace("Case summary record updated with queue item reference: " + newQueueItem.Id);


                        //find the child entity table name and primary key column from service configuration table
                        QueryExpression serviceConfigQuery = new QueryExpression("ss_serviceconfiguration")
                        {
                            ColumnSet = new ColumnSet("ss_servicelogicalname", "ss_serviceprimarykeycolumnname")

                        };

                        serviceConfigQuery.Criteria.AddCondition("ss_serviceconfigurationid", ConditionOperator.Equal, incidentAttributes.GetAttributeValue<EntityReference>("ss_serviceconfigurationid").Id);

                        var result = service.RetrieveMultiple(serviceConfigQuery);

                        var serviceConfig = result.Entities.Count() > 0 ? result.Entities[0] : null;

                        tracingService.Trace("Service configuration record retrieved: " + (serviceConfig != null ? "Yes" : "No"));

                        if (serviceConfig == null)
                        {
                            throw new InvalidPluginExecutionException("Service configuration record not found for the given service configuration id.");
                        }

                        var serviceLogicalName = serviceConfig.GetAttributeValue<string>("ss_servicelogicalname");
                        var primaryColumnName = serviceConfig.GetAttributeValue<string>("ss_serviceprimarykeycolumnname");

                        if (string.IsNullOrWhiteSpace(serviceLogicalName))
                        {
                            throw new InvalidPluginExecutionException(
                                "Service logical name is empty in Service Configuration.");
                        }

                        if (string.IsNullOrWhiteSpace(primaryColumnName))
                        {
                            throw new InvalidPluginExecutionException(
                                "Primary column name is empty in Service Configuration.");
                        }

                        tracingService.Trace(serviceLogicalName + " - " + primaryColumnName);
                        tracingService.Trace("Got to here");


                        QueryExpression childEntityQuery = new QueryExpression(serviceLogicalName)
                        {
                            ColumnSet = new ColumnSet(true)
                        };

                        childEntityQuery.Criteria.AddCondition(primaryColumnName, ConditionOperator.Equal, incidentAttributes.GetAttributeValue<string>("nulbc_casenumber"));

                        var childEntityResult = service.RetrieveMultiple(childEntityQuery);

                        Entity childEntity = childEntityResult.Entities.Count() > 0 ? childEntityResult.Entities[0] : null;

                        if (childEntity == null)
                        {
                            throw new InvalidPluginExecutionException("Child entity record not found");
                        }

                        var ChildEntityToUpdate = new Entity(serviceLogicalName)
                        {
                            Id = childEntity.Id
                        };


                        EntityToUpdate["nulbc_queueitem"] = new EntityReference(QueueItem.TableName, newQueueItem.Id);

                        service.Update(EntityToUpdate);

                    }
                }
                //If Case summary entity already has associated queue item
                else if (currentRecord.GetAttributeValue<EntityReference>("nulbc_queueitem") != null && currentRecord.GetAttributeValue<EntityReference>("nulbc_queueitem").Id != Guid.Empty)
                {
                    tracingService.Trace("queue item is not null");

                    var queueItemId = currentRecord.GetAttributeValue<EntityReference>("nulbc_queueitem").Id;
                    Entity incidentAttributes = RetrieveCaseAttributes(service, recordId);

                    //update queue item with updated case entity attributes
                    UpdateQueueItem(service, tracingService, entity, queueItemId, incidentAttributes);
                }

            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new InvalidPluginExecutionException("Fault exception occured executing QueueItemHandler: " + ex + ".");
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An exception occured executing QueueItemHandler: " + ex + ".");
            }



        }

        private static void UpdateQueueItem(IOrganizationService service, ITracingService tracingService, Entity entity, Guid queueItemId, Entity incidentAttributes)
        {

            DateTime? dateCompleted = incidentAttributes.GetAttributeValue<DateTime?>("nulbc_datecompleted").HasValue ? incidentAttributes.GetAttributeValue<DateTime?>("nulbc_datecompleted") : null;

            tracingService.Trace("executing UpdateQueueItem method");
            try
            {
                var queueItemToUpdate = new Entity(QueueItem.TableName)
                {
                    Id = queueItemId
                };


                //if (entity.Attributes.Contains("nulbc_datecompleted") && incidentAttributes["nulbc_datecompleted"] != null)
                //{
                //    queueItemToUpdate[QueueItem.DateCompleted] = incidentAttributes.GetAttributeValue<DateTime>("nulbc_datecompleted");
                //}

                if (incidentAttributes.GetAttributeValue<EntityReference>("ss_customerid") != null)
                {
                    queueItemToUpdate[QueueItem.Contact] = new EntityReference("contact", incidentAttributes.GetAttributeValue<EntityReference>("ss_customerid").Id);

                }


                queueItemToUpdate[QueueItem.Address] = incidentAttributes.GetAttributeValue<string>("nulbc_address");


                queueItemToUpdate[QueueItem.StatusFilter] = new OptionSetValue(incidentAttributes.GetAttributeValue<OptionSetValue>("nulbc_status").Value);


                queueItemToUpdate[QueueItem.FirstName] = incidentAttributes.GetAttributeValue<string>("nulbc_firstname");


                queueItemToUpdate[QueueItem.LastName] = incidentAttributes.GetAttributeValue<string>("nulbc_lastname");


                queueItemToUpdate[QueueItem.PhoneNumber] = incidentAttributes.GetAttributeValue<string>("nulbc_phonenumber");


                queueItemToUpdate[QueueItem.EmailAddress] = incidentAttributes.GetAttributeValue<string>("nulbc_emailaddress");


                queueItemToUpdate[QueueItem.ExternalReference] = incidentAttributes.GetAttributeValue<string>("nulbc_externalreference");


                queueItemToUpdate[QueueItem.InternalNotification] = new OptionSetValue(incidentAttributes.GetAttributeValue<OptionSetValue>("nulbc_internalnotification").Value.GetHashCode());


                queueItemToUpdate[QueueItem.InternalNotificationEmail] = incidentAttributes.GetAttributeValue<string>("nulbc_internalnotificationemail");


                queueItemToUpdate[QueueItem.InternalNotificationURL] = incidentAttributes.GetAttributeValue<string>("nulbc_internalnotificationurl");


                if (incidentAttributes.GetAttributeValue<OptionSetValue>("nulbc_customerstatus") != null)
                {
                    queueItemToUpdate[QueueItem.CustomerStatus] = new OptionSetValue(incidentAttributes.GetAttributeValue<OptionSetValue>("nulbc_customerstatus").Value.GetHashCode());
                }

                if (incidentAttributes.GetAttributeValue<OptionSetValue>("nulbc_datasource") != null)
                {
                    queueItemToUpdate[QueueItem.DataSource] = new OptionSetValue(incidentAttributes.GetAttributeValue<OptionSetValue>("nulbc_datasource").Value.GetHashCode());

                }

                queueItemToUpdate[QueueItem.Description] = incidentAttributes.GetAttributeValue<string>("nulbc_description");



                if (incidentAttributes.GetAttributeValue<EntityReference>("ss_subjectid") != null)
                {
                    queueItemToUpdate[QueueItem.Subject] = new EntityReference("subject", incidentAttributes.GetAttributeValue<EntityReference>("ss_subjectid").Id);

                }


                //For testing purposes, hardcoding the queue id to the 'Unassigned' queue
                queueItemToUpdate[QueueItem.Queue] = new EntityReference("queue", incidentAttributes.GetAttributeValue<EntityReference>("nulbc_queue").Id);


  
                //queueItemToUpdate[QueueItem.CaseStatus] = new EntityReference("nulbc_statuscase", incidentAttributes.GetAttributeValue<OptionSetValue>("nulbc_statuscase"));


                queueItemToUpdate[QueueItem.Owner] = new EntityReference("systemuser", incidentAttributes.GetAttributeValue<EntityReference>("ownerid").Id);


                queueItemToUpdate[QueueItem.DateCompleted] = dateCompleted.HasValue ? dateCompleted.Value : (DateTime?)null;

                service.Update(queueItemToUpdate);
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("Fault exception occured updating queue item: " + ex + ".");
            }
        }

        private static void CreateQueueItem(IOrganizationService service, ITracingService tracingService, Guid recordId, Entity unassignedUser, Guid queueId, Entity incidentAttributes)
        {

            DateTime? dateSubmitted = incidentAttributes.GetAttributeValue<DateTime?>("nulbc_datesubmitted").HasValue ? incidentAttributes.GetAttributeValue<DateTime?>("nulbc_datesubmitted") : null;
            DateTime? dateCompleted = incidentAttributes.GetAttributeValue<DateTime?>("nulbc_datecompleted").HasValue ? incidentAttributes.GetAttributeValue<DateTime?>("nulbc_datecompleted") : null;
            //create a queue item for the Case summary entity2
            try
            {
                var entityToCreate = new Entity(QueueItem.TableName);

                if (incidentAttributes.GetAttributeValue<EntityReference>("ss_customerid") != null)
                {
                    entityToCreate[QueueItem.Contact] = new EntityReference("contact", incidentAttributes.GetAttributeValue<EntityReference>("ss_customerid").Id);

                }

                if (incidentAttributes.GetAttributeValue<EntityReference>("ss_servicerequestid") != null)
                {
                    tracingService.Trace("service request id value: " + incidentAttributes.GetAttributeValue<EntityReference>("ss_servicerequestid"));
                    entityToCreate[QueueItem.Object] = incidentAttributes.GetAttributeValue<EntityReference>("ss_servicerequestid");
                }


                entityToCreate[QueueItem.Address] = incidentAttributes.GetAttributeValue<string>("nulbc_address");


                entityToCreate[QueueItem.StatusFilter] = new OptionSetValue(StatusFilter.RequiresAction.GetHashCode());


                entityToCreate[QueueItem.FirstName] = incidentAttributes.GetAttributeValue<string>("nulbc_firstname");


                entityToCreate[QueueItem.LastName] = incidentAttributes.GetAttributeValue<string>("nulbc_lastname");


                entityToCreate[QueueItem.PhoneNumber] = incidentAttributes.GetAttributeValue<string>("nulbc_phonenumber");


                entityToCreate[QueueItem.EmailAddress] = incidentAttributes.GetAttributeValue<string>("nulbc_emailaddress");


                entityToCreate[QueueItem.ExternalReference] = incidentAttributes.GetAttributeValue<string>("nulbc_externalreference");


                entityToCreate[QueueItem.InternalNotification] = new OptionSetValue(incidentAttributes.GetAttributeValue<OptionSetValue>("nulbc_internalnotification").Value.GetHashCode());


                entityToCreate[QueueItem.InternalNotificationEmail] = incidentAttributes.GetAttributeValue<string>("nulbc_internalnotificationemail");


                entityToCreate[QueueItem.InternalNotificationURL] = incidentAttributes.GetAttributeValue<string>("nulbc_internalnotificationurl");


                if (incidentAttributes.GetAttributeValue<OptionSetValue>("nulbc_customerstatus") != null)
                {
                    entityToCreate[QueueItem.CustomerStatus] = new OptionSetValue(incidentAttributes.GetAttributeValue<OptionSetValue>("nulbc_customerstatus").Value);

                }

                if (incidentAttributes.GetAttributeValue<OptionSetValue>("nulbc_datasource") != null)
                {
                    entityToCreate[QueueItem.DataSource] = new OptionSetValue(incidentAttributes.GetAttributeValue<OptionSetValue>("nulbc_datasource").Value.GetHashCode());

                }

                entityToCreate[QueueItem.Description] = incidentAttributes.GetAttributeValue<string>("nulbc_description");



                if (incidentAttributes.GetAttributeValue<EntityReference>("ss_subjectid") != null)
                {
                    entityToCreate[QueueItem.Subject] = new EntityReference("subject", incidentAttributes.GetAttributeValue<EntityReference>("ss_subjectid").Id);

                }


                //For testing purposes, hardcoding the queue id to the 'Unassigned' queue
                entityToCreate[QueueItem.Queue] = new EntityReference("queue", queueId);


                //entityToCreate[QueueItem.Queue] = new EntityReference("queue", incidentAttributes.GetAttributeValue<EntityReference>("nulbc_queue").Id);

                //entityToCreate[QueueItem.CaseStatus] = new EntityReference("nulbc_statuscase", incidentAttributes.GetAttributeValue<OptionSetValue>("nulbc_statuscase"));

                //entityToCreate[QueueItem.Object] = new EntityReference(Case.TableName, recordId);


                entityToCreate[QueueItem.Owner] = new EntityReference("systemuser", unassignedUser.Id);


                entityToCreate[QueueItem.Title] = incidentAttributes.GetAttributeValue<string>("ss_casenumber");


                entityToCreate[QueueItem.DateSubmitted] = dateSubmitted.Value;


                entityToCreate[QueueItem.DateCompleted] = dateCompleted.HasValue ? dateCompleted.Value : (DateTime?)null;


                //if (Convert.ToString(currentRecord.GetAttributeValue<DateTime>("nulbc_datesubmitted")) != "")
                //{
                //    entityToCreate[QueueItem.DateSubmitted] = dateSubmitted.Value;
                //}

                //if (dateCompleted.HasValue)
                //{
                //    entityToCreate[QueueItem.DateCompleted] = dateCompleted.Value;
                //}

                service.Create(entityToCreate);

            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("Fault exception occured creating queue item record: " + ex + ".");
            }
        }

        private static Entity RetrieveCaseAttributes(IOrganizationService service, Guid recordId)
        {
            //retrieve the rest of the common attributes of the Case summary entity to create a queue item
            return service.Retrieve(
                Case.TableName,
                recordId,
                new ColumnSet(
                    "ss_customerid",
                    "nulbc_address",
                    "nulbc_status",
                    "nulbc_casereference",
                    "nulbc_firstname",
                    "nulbc_lastname",
                    "nulbc_emailaddress",
                    "nulbc_description",
                    "nulbc_datecompleted",
                    "nulbc_datasource",
                    "nulbc_casestatus",
                    "nulbc_customerstatus",
                    "nulbc_externalreference",
                    "nulbc_internalnotification",
                    "nulbc_internalnotificationemail",
                    "nulbc_internalnotificationurl",
                    "nulbc_phonenumber",
                    "nulbc_property",
                    "ss_subjectid",
                    "nulbc_synceddate",
                    "nulbc_queue",
                    "nulbc_datesubmitted",
                    "nulbc_queueitem",
                    "nulbc_datecompleted",
                    "ss_servicerequestid"
                )
            );
        }
    }
}

