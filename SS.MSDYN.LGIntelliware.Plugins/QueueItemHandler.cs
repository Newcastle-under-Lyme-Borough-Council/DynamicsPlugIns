using Microsoft.Xrm.Sdk;
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
            localContext.TracingService.Trace("UnsecuredString - nulbc_case");

            if (localContext == null)
            {
                throw new ArgumentNullException(nameof(localContext));
            }
            var context = localContext.PluginExecutionContext;
            var service = localContext.OrganizationService;
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


                // Check if context has 'Target' input parameter...
                if (context.InputParameters.ContainsKey(ContextInputParameters.TARGET) && context.InputParameters[ContextInputParameters.TARGET] != null)
                {
                    throw new InvalidPluginExecutionException("Target input parameter is missing or null");
                }

                var entity = (Entity)context.InputParameters[ContextInputParameters.TARGET];

                // Check if entity reference is of type Case summary...
                if (!entity.LogicalName.Equals(Case.TableName))
                {
                    throw new InvalidPluginExecutionException("Entity calling plugin is not Case summary entity.");
                }
                //Get record id of Case summary entity
                Guid recordId = entity.Id;

                //Get relevantcolumn values of entity - queueitem, queue and date submitted
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
                EntityReference queue = currentRecord.GetAttributeValue<EntityReference>("nulbc_queue");
                //DateTime is a value type and cannot be null
                DateTime? dateSubmitted = currentRecord.GetAttributeValue<DateTime?>("nulbc_datesubmitted");

                //Check if the Case summary entity has a date submitted value
                if (!dateSubmitted.HasValue)
                {
                    localContext.TracingService.Trace("Incident has not been submitted");
                    return;
                }

                //Check if the Case summary entity has a queue reference
                if (currentRecord.GetAttributeValue<EntityReference>("nulbc_queue") == null && currentRecord.GetAttributeValue<EntityReference>("nulbc_queue").Id == Guid.Empty)
                {
                    localContext.TracingService.Trace("Incident type has no assigned queue");
                    return;
                }


                localContext.TracingService.Trace($"Incident has a date submitted: {dateSubmitted.Value}");

                //Check if the Case summary entity has no associated queue item reference
                if (currentRecord.GetAttributeValue<EntityReference>("nulbc_queueitem") == null)
                {

                    localContext.TracingService.Trace("Incident has no queue item reference");

                    //query User table to get Unassigned user
                    var unassignedOwnerQuery = new QueryExpression("systemuser")
                    {
                        ColumnSet = new ColumnSet("systemuserid", "fullname")
                    };

                    unassignedOwnerQuery.Criteria.AddCondition(
                        "fullname", ConditionOperator.Equal, "Unassigned #"
                        );

                    var users = service.RetrieveMultiple(unassignedOwnerQuery);

                    if (users.Entities.Count == 0)
                    {
                        throw new InvalidPluginExecutionException("Unassigned user not found in systemuser table.");
                    }

                    var unassignedUser = users.Entities[0];

                    localContext.TracingService.Trace($"User Id: {unassignedUser.Id}");

                    localContext.TracingService.Trace(
                        $"Full Name: {unassignedUser.GetAttributeValue<string>("fullname")}");



                    var queueId = currentRecord.GetAttributeValue<EntityReference>("nulbc_queue").Id;

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
                        localContext.TracingService.Trace("Incident has a valid queue reference");

                        //retrieve the rest of the common attributes of the Case summary entity to create a queue item
                        var incidentAttributes = service.Retrieve(
                            Case.TableName,
                            recordId,
                            new ColumnSet(
                                "nulbc_contact",
                                "nulbc_address",
                                "nulbc_status",
                                "nulbc_casereference",
                                "nulbc_firstname",
                                "nulbc_lastname"
                                )
                        );

                        //create a queue item for the Case summary entity
                        try
                        {
                            var entityToCreate = new Entity(QueueItem.TableName);
                            entityToCreate[QueueItem.Contact] = incidentAttributes.GetAttributeValue<string>("nulbc_contact");
                            entityToCreate[QueueItem.Address] = incidentAttributes.GetAttributeValue<string>("nulbc_address");
                            entityToCreate[QueueItem.StatusFilter] = incidentAttributes.GetAttributeValue<string>("nulbc_status");
                            entityToCreate[QueueItem.CaseReference] = incidentAttributes.GetAttributeValue<string>("nulbc_casereference");
                            entityToCreate[QueueItem.FirstName] = incidentAttributes.GetAttributeValue<string>("nulbc_firstname");
                            entityToCreate[QueueItem.LastName] = incidentAttributes.GetAttributeValue<string>("nulbc_lastname");
                            entityToCreate[QueueItem.Queue] = incidentAttributes.GetAttributeValue<string>("nulbc_queue");
                            entityToCreate[QueueItem.Object] = new EntityReference(Case.TableName, recordId);


                            service.Create(entityToCreate);
                        }
                        catch (Exception ex)
                        {
                            throw new InvalidPluginExecutionException("Fault exception occured creating queue item record: " + ex + ".");
                        }

                    }
                    //If Case summary entity already has associated queue item
                    else if (currentRecord.GetAttributeValue<EntityReference>("nulbc_queueitem") != null && currentRecord.GetAttributeValue<EntityReference>("nulbc_queueitem").Id != Guid.Empty)
                    {
                        var queueItemId = currentRecord.GetAttributeValue<EntityReference>("nulbc_queueitem").Id;
                        localContext.TracingService.Trace("Incident has an associated queue item");

                        //update queue item with updated case entity attributes
                    }
                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new InvalidPluginExecutionException("Fault exception occured executing PostCreate: " + ex + ".");
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An exception occured executing PostCreate: " + ex + ".");
            }

        }
    }
}