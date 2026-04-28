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
    public class PostCreate : PluginBase
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        protected PluginConfig Config = new PluginConfig();

        public PostCreate(string unsecureConfiguration)
           : base(typeof(PostOrderNewRecycling), unsecureConfiguration) 
        {
            if (!string.IsNullOrWhiteSpace(unsecureConfiguration))
            {
                Config = JsonSerializer.Deserialize<PluginConfig>(unsecureConfiguration);
            }
            RegisterEvents(); 
        }
        //Registers the plugin to run after a missedbin record is created.
        public PostCreate() : base(typeof(PostMissedBin))
        {
            RegisterEvents();
        }

        public void RegisterEvents()
        {
            RegisteredEvents.Add(new Tuple<int, string, string, Action<LocalPluginContext>>(PluginExecutionPipelineStage.PostOperation.GetHashCode(), PluginExecutionMessageName.CREATE, Config.TableName, Execute));
        }

        /// <summary>
        /// Executes plug-in code in response to an event.
        /// </summary>
        /// <param name="localContext">Contains a local plug-in context.</param>
        protected void Execute(LocalPluginContext localContext)
        {
            localContext.TracingService.Trace("UnsecuredString - " + Config.TableName);

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
                            // Check if entity reference is of type missed bin...
                            if (entity.LogicalName.Equals(Config.TableName))
                            {
                                // Check if the entity has a service configuration reference
                                if (entity.Attributes.Contains(Config.ServiceConfigurationColumnLogicalName) &&
                                 ((EntityReference)entity.Attributes[Config.ServiceConfigurationColumnLogicalName]).Id != Guid.Empty)
                                {
                                    var serviceConfigurationId = entity.GetAttributeValue<EntityReference>(Config.ServiceConfigurationColumnLogicalName).Id;
                                    var serviceConfigurations = DataverseHelper.RetrieveServiceConfiguration(service, ServiceConfiguration.TableName, serviceConfigurationId, new ColumnSet(ServiceConfiguration.Subject, ServiceConfiguration.Name));
                                    if (serviceConfigurations != null && serviceConfigurations.Entities != null && serviceConfigurations.Entities.Count > 0)
                                    {
                                        var serviceConfiguration = serviceConfigurations.Entities[0];
                                        if (serviceConfiguration.Attributes.ContainsKey(ServiceConfiguration.Subject) && serviceConfiguration.Attributes[ServiceConfiguration.Subject] != null)
                                        {
                                            var title = serviceConfiguration.GetAttributeValue<string>(ServiceConfiguration.Subject);
                                            var subjects = DataverseHelper.RetrieveSubject(service, Subject.TableName, title, new ColumnSet(Subject.Title));
                                            if (subjects != null && subjects.Entities != null && subjects.Entities.Count > 0)
                                            {
                                                var subject = subjects.Entities[0];
                                                // Create an Incident (case) for the missed bin
                                                var incidentId = DataverseHelper.CreateCaseGeneric(service, entity, serviceConfiguration, subject, Config,localContext.TracingService);

                                                Entity entityToUpdate = new Entity(entity.LogicalName)
                                                {
                                                    Id = entity.Id
                                                };
                                                entityToUpdate.Attributes.Add(ServiceRequest.Case, new EntityReference(Case.TableName, incidentId));
                                                DataverseHelper.Update(service, entityToUpdate);
                                            }
                                            else
                                            {
                                                throw new InvalidPluginExecutionException("There are no active subject found for the selected missed collection. Please contact system administrator for more details");
                                            }
                                        }
                                        else
                                        {
                                            throw new InvalidPluginExecutionException("Selected missed collection does not have subject configuration. Please contact system administrator for more details");
                                        }
                                    }
                                    else
                                    {
                                        throw new InvalidPluginExecutionException("There are no active service configuration found for the selected missed collection. Please contact system administrator for more details");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new InvalidPluginExecutionException("Fault exception occured executing PostMissedBin: " + ex + ".");
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An exception occured executing PostMissedBin: " + ex + ".");
            }

        }
    }
}
