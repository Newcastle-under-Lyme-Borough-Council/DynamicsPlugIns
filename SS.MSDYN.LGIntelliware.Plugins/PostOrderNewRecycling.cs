using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SS.MSDYN.LGIntelliware.Plugins
{
    public class PostOrderNewRecycling : PluginBase
    {
        /// <summary>
        /// Constructor.
        /// </summary>

        //Registers the plugin to run after a Order New Recycling waste record is created.

        public PostOrderNewRecycling() : base(typeof(PostOrderNewRecycling))
        {
            RegisteredEvents.Add(new Tuple<int, string, string, Action<LocalPluginContext>>(PluginExecutionPipelineStage.PostOperation.GetHashCode(), PluginExecutionMessageName.CREATE, ServiceRequest.OrderRecyclingContainerTableName, Execute));
        }

        /// <summary>
        /// Executes plug-in code in response to an event.
        /// </summary>
        /// <param name="localContext">Contains a local plug-in context.</param>
        protected void Execute(LocalPluginContext localContext)
        {
            if (localContext == null) throw new ArgumentNullException(nameof(localContext));
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
                            // Check if entity reference is of type PostOrderNewRecycling...
                            if (entity.LogicalName.Equals(ServiceRequest.OrderRecyclingContainerTableName))
                            {
                                if (entity.Attributes.Contains(ServiceRequest.ServiceConfiguration) &&
                                ((EntityReference)entity.Attributes[ServiceRequest.ServiceConfiguration]).Id != Guid.Empty)
                                {
                                    var serviceConfigurationId = entity.GetAttributeValue<EntityReference>(ServiceRequest.ServiceConfiguration).Id;
                                    var serviceConfigurations = DataverseHelper.RetrieveServiceConfiguration(service, ServiceConfiguration.TableName, serviceConfigurationId, new ColumnSet(ServiceConfiguration.Subject, ServiceConfiguration.Name));
                                    // Check if the PostOrderNewRecyclinghas a service configuration
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
                                                // Create Incident (case) for garden waste
                                                var incidentId = DataverseHelper.CreateIncident(service, entity, serviceConfiguration, subject);

                                                Entity entityToUpdate = new Entity(entity.LogicalName)
                                                {
                                                    Id = entity.Id
                                                };
                                                entityToUpdate.Attributes.Add(ServiceRequest.Case, new EntityReference(Incident.TableName, incidentId));
                                                DataverseHelper.Update(service, entityToUpdate);
                                            }
                                            else
                                            {
                                                throw new InvalidPluginExecutionException("There are no active subject found for the selected garden waste. Please contact system administrator for more details");
                                            }
                                        }
                                        else
                                        {
                                            throw new InvalidPluginExecutionException("Selected garden waste does not have subject configuration. Please contact system administrator for more details");
                                        }
                                    }
                                    else
                                    {
                                        throw new InvalidPluginExecutionException("There are no active service configuration found for the selected garden waste. Please contact system administrator for more details");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new InvalidPluginExecutionException("Fault exception occured executing PostGardenWaste: " + ex + ".");
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An exception occured executing PostGardenWaste: " + ex + ".");
            }

        }
    }
}
