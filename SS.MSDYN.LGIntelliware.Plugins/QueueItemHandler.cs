using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using SS.MSDYN.LGIntelliware.Plugins.Common;
using System;
using System.Collections.Generic;
using System.Linq;
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
            RegisteredEvents.Add(new Tuple<int, string, string, Action<LocalPluginContext>>(PluginExecutionPipelineStage.PostOperation.GetHashCode(), PluginExecutionMessageName.CREATE, "ss_incident", Execute));
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
            if (context.Depth > 1)
            {
                return;
            }
            try
            {
                // Check if context message name is 'Create' ...
                if (context.MessageName.Equals(PluginExecutionMessageName.CREATE))
                {
                    // Check if context has 'Target' input parameter...
                    if (context.InputParameters.ContainsKey(ContextInputParameters.TARGET) && context.InputParameters[ContextInputParameters.TARGET] != null)
                    {
                        var entity = (Entity)context.InputParameters[ContextInputParameters.TARGET];
                        if (entity != null)
                        {
                            // Check if entity reference is of type Case summary...
                            if (entity.LogicalName.Equals("ss_incident"))
                            {
                                // Check if the Case summary entity has a queue reference
                                if (entity.Attributes.Contains("nulbc_queue") &&
                                 ((EntityReference)entity.Attributes["nulbc_queue"]).Id != Guid.Empty)
                                {
                                    var queueId = entity.GetAttributeValue<EntityReference>("nulbc_queue").Id;
                                    localContext.TracingService.Trace("Incident has an assigned queue");

                                }
                                else
                                {
                                    localContext.TracingService.Trace("Incident type has no queue assigned");
                                }
                            }
                            else
                            {
                                throw new InvalidPluginExecutionException("Entity calling plugin is not Case summary entity.");
                            }
                        }
                        else
                        {
                            throw new InvalidPluginExecutionException("Entity has null value");
                        }
                    }
                    else
                    {
                        throw new InvalidPluginExecutionException("Target input parameter is missing or null");
                    }
                }
                //Check if context message name is 'Update'...
                else if (context.MessageName.Equals(PluginExecutionMessageName.UPDATE))
                { 
                    // Check if context has 'Target' input parameter...
                    if (context.InputParameters.ContainsKey(ContextInputParameters.TARGET) && context.InputParameters[ContextInputParameters.TARGET] != null)
                    {
                        var entity = (Entity)context.InputParameters[ContextInputParameters.TARGET];
                        if (entity != null)
                        {
                            // Check if entity reference is of type incident...
                            if (entity.LogicalName.Equals("ss_incident"))
                            {
                                localContext.TracingService.Trace("Call to update incident entity");
                            }
                            else if (entity.LogicalName.Equals("queueitem"))
                            {
                                localContext.TracingService.Trace("Call to update queue item entity");
                            }
                        }
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
              