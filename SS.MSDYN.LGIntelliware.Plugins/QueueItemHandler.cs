using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using SS.MSDYN.LGIntelliware.Plugins.Common;
using System;
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

        public QueueItemHandler(string unsecureConfiguration)
          : base(typeof(QueueItemHandler), unsecureConfiguration)
        {
            if (!string.IsNullOrWhiteSpace(unsecureConfiguration))
            {
                Config = JsonSerializer.Deserialize<PluginConfig>(unsecureConfiguration);
            }
            RegisterEvents();
        }
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

                // Check if entity reference is of type Case summary...
                if (!entity.LogicalName.Equals(Case.TableName))
                {
                    throw new InvalidPluginExecutionException("Entity calling plugin is not Case summary entity.");
                }

                tracingService.Trace("Entity is Case summary");
                //Get record id of Case summary entity
                Guid recordId = entity.Id;

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

                EntityReference queueItem = currentRecord.GetAttributeValue<EntityReference>("nulbc_queueitem");

                Guid queueItemGuid = queueItem != null ? queueItem.Id : Guid.Empty;
                tracingService.Trace($"Queue item id: {queueItemGuid}");
                EntityReference queue = currentRecord.GetAttributeValue<EntityReference>("nulbc_queue");
                //DateTime is a value type and cannot be null
                DateTime? dateSubmitted = currentRecord.GetAttributeValue<DateTime?>("nulbc_datesubmitted");

                //Check if the Case summary entity has a date submitted value
                if (!dateSubmitted.HasValue)
                {
                    tracingService.Trace("Incident has not been submitted");
                    return;
                }

                //Check if the Case summary entity has a queue reference
                //if (currentRecord.GetAttributeValue<EntityReference>("nulbc_queue") == null && currentRecord.GetAttributeValue<EntityReference>("nulbc_queue").Id == Guid.Empty)
                //{
                //    tracingService.Trace("Incident type has no assigned queue");
                //    return;
                //}

                tracingService.Trace($"Incident has a date submitted: {dateSubmitted.Value}");

                //Check if the Case summary entity has no associated queue item reference
                if (currentRecord.GetAttributeValue<EntityReference>("nulbc_queueitem") == null)
                {

                    tracingService.Trace("Incident has no queue item reference");

                    //Retrieve Unassigned User from User table
                    var unassignedUser = DataverseHelper.RetrieveUnassignedUser(service);

                    tracingService.Trace($"Unassigned user retrieved: {unassignedUser.Id}");
                    tracingService.Trace($"Unassigned user full name: {unassignedUser.GetAttributeValue<string>("fullname")}");


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
                        var incidentAttributes = service.Retrieve(
                            Case.TableName,
                            recordId,
                            new ColumnSet(
                                "ss_customerid",
                                "nulbc_address",
                                "nulbc_status",
                                "nulbc_casereference",
                                "ss_casenumber",
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
                                "ss_serviceconfigurationid",
                                "ss_subjectid",
                                "nulbc_synceddate",
                                "ss_serviceconfigurationid"
                            )
                        );

                        DateTime? dateCompleted = incidentAttributes.GetAttributeValue<DateTime?>("nulbc_datecompleted");

                        tracingService.Trace("Date completed value: " + dateCompleted.HasValue);

                        //create a queue item for the Case summary entity2
                        try
                        {
                            var entityToCreate = new Entity(QueueItem.TableName);
                            entityToCreate[QueueItem.Contact] = new EntityReference("contact", incidentAttributes.GetAttributeValue<EntityReference>("ss_customerid").Id);
                            tracingService.Trace($"Contactretrieved: {incidentAttributes.GetAttributeValue<EntityReference>("ss_customerid").Id}");
                            entityToCreate[QueueItem.Address] = incidentAttributes.GetAttributeValue<string>("nulbc_address");
                            entityToCreate[QueueItem.StatusFilter] = new OptionSetValue(StatusFilter.RequiresAction.GetHashCode());
                            entityToCreate[QueueItem.FirstName] = incidentAttributes.GetAttributeValue<string>("nulbc_firstname");
                            entityToCreate[QueueItem.LastName] = incidentAttributes.GetAttributeValue<string>("nulbc_lastname");
                            entityToCreate[QueueItem.PhoneNumber] = incidentAttributes.GetAttributeValue<string>("nulbc_phonenumber");
                            entityToCreate[QueueItem.EmailAddress] = incidentAttributes.GetAttributeValue<string>("nulbc_emailaddress");
                            entityToCreate[QueueItem.ExternalReference] = incidentAttributes.GetAttributeValue<string>("nulbc_externalreference");
                            entityToCreate[QueueItem.InternalNotification] = new OptionSetValue(incidentAttributes.GetAttributeValue<OptionSetValue>("nulbc_internalnotification").Value.GetHashCode());
                            entityToCreate[QueueItem.InternalNotificationEmail] = incidentAttributes.GetAttributeValue<OptionSetValue>("nulbc_internalnotificationemail");
                            entityToCreate[QueueItem.InternalNotificationURL] = incidentAttributes.GetAttributeValue<OptionSetValue>("nulbc_internalnotificationurl");
                            entityToCreate[QueueItem.CustomerStatus] = new OptionSetValue(incidentAttributes.GetAttributeValue<OptionSetValue>("nulbc_customerstatus").Value.GetHashCode());
                            entityToCreate[QueueItem.DataSource] = new OptionSetValue(incidentAttributes.GetAttributeValue<OptionSetValue>("nulbc_datasource").Value.GetHashCode());
                            entityToCreate[QueueItem.Description] = incidentAttributes.GetAttributeValue<string>("nulbc_description");



                            //For testing purposes, hardcoding the queue id to the 'Unassigned' queue
                            entityToCreate[QueueItem.Queue] = new EntityReference("queue", queueId);
                            //entityToCreate[QueueItem.Queue] = new EntityReference("queue", incidentAttributes.GetAttributeValue<EntityReference>("nulbc_queue").Id);
                            //entityToCreate[QueueItem.CaseStatus] = new EntityReference("nulbc_statuscase", incidentAttributes.GetAttributeValue<OptionSetValue>("nulbc_statuscase").Id);

                            entityToCreate[QueueItem.Object] = new EntityReference(Case.TableName, recordId);
                            entityToCreate[QueueItem.Owner] = new EntityReference("systemuser", unassignedUser.Id);
                            entityToCreate[QueueItem.Title] = incidentAttributes.GetAttributeValue<string>("ss_casenumber");
                            entityToCreate[QueueItem.DateSubmitted] = dateSubmitted.Value;
                            entityToCreate[QueueItem.DateCompleted] = dateCompleted.HasValue ? dateCompleted.Value : (DateTime?)null;


                            tracingService.Trace("Date submitted value: " + Convert.ToString(currentRecord.GetAttributeValue<DateTime>("nulbc_datesubmitted")));
                            tracingService.Trace("Date completed value: " + Convert.ToString(currentRecord.GetAttributeValue<DateTime>("nulbc_datecompleted")));

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
                }
                //If Case summary entity already has associated queue item
                else if (currentRecord.GetAttributeValue<EntityReference>("nulbc_queueitem") != null && currentRecord.GetAttributeValue<EntityReference>("nulbc_queueitem").Id != Guid.Empty)
                {
                    tracingService.Trace("queue item is not null");

                    var queueItemId = currentRecord.GetAttributeValue<EntityReference>("nulbc_queueitem").Id;



                    //retrieve the rest of the common attributes of the Case summary entity to create a queue item
                    var incidentAttributes = service.Retrieve(
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
                            "ss_serviceconfigurationid",
                            "ss_subjectid",
                            "nulbc_synceddate"
                        )
                    );

                    //update queue item with updated case entity attributes
                    try
                    {
                        var queueItemToUpdate = new Entity(QueueItem.TableName)
                        {
                            Id = queueItemId
                        };


                        if (entity.Attributes.Contains("nulbc_datecompleted") && incidentAttributes["nulbc_datecompleted"] != null)
                        {
                            queueItemToUpdate[QueueItem.DateCompleted] = incidentAttributes.GetAttributeValue<DateTime>("nulbc_datecompleted");
                        }

                        if (entity.Attributes.Contains("nulbc_emailaddress") && incidentAttributes["nulbc_emailaddress"] != null)
                        {
                            queueItemToUpdate[QueueItem.EmailAddress] = incidentAttributes.GetAttributeValue<string>("nulbc_emailaddress");
                            tracingService.Trace("Email address updated:" + incidentAttributes.GetAttributeValue<string>("nulbc_emailaddress"));
                        }

                        //queueItemToUpdate[QueueItem.Contact] = new EntityReference("contact", incidentAttributes.GetAttributeValue<EntityReference>("ss_customerid").Id);
                        queueItemToUpdate[QueueItem.Address] = incidentAttributes.GetAttributeValue<string>("nulbc_address");
                        queueItemToUpdate[QueueItem.StatusFilter] = new OptionSetValue(incidentAttributes.GetAttributeValue<OptionSetValue>("nulbc_status").Value.GetHashCode());
                        queueItemToUpdate[QueueItem.FirstName] = incidentAttributes.GetAttributeValue<string>("nulbc_firstname");
                        queueItemToUpdate[QueueItem.LastName] = incidentAttributes.GetAttributeValue<string>("nulbc_lastname");
                        queueItemToUpdate[QueueItem.PhoneNumber] = incidentAttributes.GetAttributeValue<string>("nulbc_phonenumber");
                        //queueItemToUpdate[QueueItem.EmailAddress] = incidentAttributes.GetAttributeValue<string>("nulbc_emailaddress");
                        //queueItemToUpdate[QueueItem.DateCompleted] = incidentAttributes.GetAttributeValue<DateTime>("nulbc_datecompleted");
                        queueItemToUpdate[QueueItem.DateSubmitted] = incidentAttributes.GetAttributeValue<DateTime>("nulbc_datesubmitted");
                        queueItemToUpdate[QueueItem.ExternalReference] = incidentAttributes.GetAttributeValue<string>("nulbc_externalreference");
                        queueItemToUpdate[QueueItem.InternalNotification] = new OptionSetValue(incidentAttributes.GetAttributeValue<OptionSetValue>("nulbc_internalnotification").Value.GetHashCode());
                        queueItemToUpdate[QueueItem.InternalNotificationEmail] = incidentAttributes.GetAttributeValue<OptionSetValue>("nulbc_internalnotificationemail");
                        queueItemToUpdate[QueueItem.InternalNotificationURL] = incidentAttributes.GetAttributeValue<OptionSetValue>("nulbc_internalnotificationurl");
                        queueItemToUpdate[QueueItem.CustomerStatus] = new OptionSetValue(incidentAttributes.GetAttributeValue<OptionSetValue>("nulbc_customerstatus").Value.GetHashCode());
                        queueItemToUpdate[QueueItem.DataSource] = new OptionSetValue(incidentAttributes.GetAttributeValue<OptionSetValue>("nulbc_datasource").Value.GetHashCode());
                        queueItemToUpdate[QueueItem.Description] = incidentAttributes.GetAttributeValue<string>("nulbc_description");
                        //queueItemToUpdate[QueueItem.CaseStatus] = new OptionSetValue(incidentAttributes.GetAttributeValue<OptionSetValue>("nulbc_casestatus").Value.GetHashCode());
                        //For testing purposes, hardcoding the queue id to the 'Unassigned' queue
                        //queueItemToUpdate[QueueItem.Object] = new EntityReference(Case.TableName, recordId);
                        //queueItemToUpdate[QueueItem.Owner] = new EntityReference("systemuser", unassignedUser.Id);


                        service.Update(queueItemToUpdate);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidPluginExecutionException("Fault exception occured updating queue item: " + ex + ".");
                    }
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
    }
}