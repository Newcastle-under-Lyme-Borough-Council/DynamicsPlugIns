using System;
using System.Collections.Generic;
using System.IdentityModel.Metadata;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace SS.MSDYN.LGIntelliware.Plugins
{
    public class PostMissedBin : PluginBase
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public PostMissedBin() : base(typeof(PostMissedBin))
        {
            RegisteredEvents.Add(new Tuple<int, string, string, Action<LocalPluginContext>>(PluginExecutionPipelineStage.PostOperation.GetHashCode(), PluginExecutionMessageName.CREATE, ServiceRequestTableColumnNames.MissedBinTableName, Execute));
        }

        /// <summary>
        /// Executes plug-in code in response to an event.
        /// </summary>
        /// <param name="localContext">Contains a local plug-in context.</param>
        protected void Execute(LocalPluginContext localContext)
        {
            if (localContext == null) throw new ArgumentNullException(nameof(localContext));
            var context = localContext.PluginExecutionContext;
            var tracingService = localContext.TracingService;
            var service = localContext.OrganizationService;

            try
            {
                // Check if plug-in execution pipeline stage is 'PostOperation'...
                tracingService.Trace("{0}", "Going to check plug-in execution pipeline stage.");
                if (context.Stage == PluginExecutionPipelineStage.PostOperation.GetHashCode())
                {
                    // Check if context message name is 'Create' ...
                    tracingService.Trace("{0}", "Context message name: " + context.MessageName + ".");
                    tracingService.Trace("{0}", "Going to verify execution message name.");
                    if (context.MessageName.Equals(PluginExecutionMessageName.CREATE))
                    {
                        // Check if context has 'Target' input parameter...
                        tracingService.Trace("{0}", "Going to check context for 'Target' input parameter.");
                        if (context.InputParameters.ContainsKey(ContextInputParameters.TARGET) && context.InputParameters[ContextInputParameters.TARGET] != null)
                        {
                            tracingService.Trace("{0}", "Context has 'Target' input parameter.");
                            var entity = (Entity)context.InputParameters[ContextInputParameters.TARGET];
                            if (entity != null)
                            {
                                tracingService.Trace("{0}", "'Target' entity from context input parameter Guid: " + entity.Id + ".");
                                // Check if entity reference is of type missed bin...
                                tracingService.Trace("{0}", "Going to check entity reference logical name.");
                                if (entity.LogicalName.Equals(ServiceRequestTableColumnNames.MissedBinTableName))
                                {
                                    tracingService.Trace("{0}", "'Target' entity logical name: " + entity.LogicalName + ".");
                                    if (entity.Attributes.ContainsKey(ServiceRequestTableColumnNames.ServiceConfiguration) && entity.Attributes[ServiceRequestTableColumnNames.ServiceConfiguration] != null)
                                    {
                                        var serviceConfigurationId = entity.GetAttributeValue<EntityReference>(ServiceRequestTableColumnNames.ServiceConfiguration).Id;
                                        tracingService.Trace("{0}", "Service configuration 'ID' is: " + serviceConfigurationId + ".");
                                        var serviceConfigurations = DataverseHelper.RetrieveServiceConfiguration(service, tracingService, ServiceConfigurationTableColumnNames.TableName, serviceConfigurationId, new ColumnSet(ServiceConfigurationTableColumnNames.Subject, ServiceConfigurationTableColumnNames.Name));
                                        if (serviceConfigurations != null && serviceConfigurations.Entities != null && serviceConfigurations.Entities.Count > 0)
                                        {
                                            tracingService.Trace("{0}", "Getting first service configuration record from serviceConfigurations entity collections.");
                                            var serviceConfiguration = serviceConfigurations.Entities[0];
                                            tracingService.Trace("{0}", "Check serviceConfiguration contains 'Subject' attribute:");
                                            if (serviceConfiguration.Attributes.ContainsKey(ServiceConfigurationTableColumnNames.Subject) && serviceConfiguration.Attributes[ServiceConfigurationTableColumnNames.Subject] != null)
                                            {
                                                var title = serviceConfiguration.GetAttributeValue<string>(ServiceConfigurationTableColumnNames.Subject);
                                                tracingService.Trace("{0}", "Service configuration 'Title' is: " + title + ".");
                                                tracingService.Trace("{0}", "Going to retrieve subject.");
                                                var subjects = DataverseHelper.RetrieveSubject(service, tracingService, SubjectTableColumnNames.TableName, title, new ColumnSet(SubjectTableColumnNames.Title));
                                                if (subjects != null && subjects.Entities != null && subjects.Entities.Count > 0)
                                                {
                                                    var subject = subjects.Entities[0];
                                                    tracingService.Trace("{0}", "Start creating new 'MissedBin' incident.");
                                                    var incidentId = DataverseHelper.CreateIncident(service, tracingService, entity, serviceConfiguration, subject);
                                                    tracingService.Trace("{0}", "Newly created missed bin incident 'ID' is: " + incidentId);
                                                    tracingService.Trace("{0}", "Going to update 'MissedBin' serice request for incident attribute.");

                                                    Entity entityToUpdate = new Entity(entity.LogicalName)
                                                    {
                                                        Id = entity.Id
                                                    };
                                                    entityToUpdate.Attributes.Add(ServiceRequestTableColumnNames.Case, new EntityReference(IncidentTableColumnNames.TableName, incidentId));
                                                    DataverseHelper.Update(service, tracingService, entityToUpdate);
                                                    tracingService.Trace("{0}", "Exiting from 'PostMissedBin' execute method after.");
                                                }
                                                else
                                                {
                                                    tracingService.Trace("{0}", "'Subject' does not have any record.");
                                                    throw new InvalidPluginExecutionException("There are no active subject found for the selected missed collection. Please contact system administrator for more details");
                                                }
                                            }
                                            else
                                            {
                                                tracingService.Trace("{0}", "'ServiceConfigurations' does not have subject attribute.");
                                                throw new InvalidPluginExecutionException("Selected missed collection does not have subject configuration. Please contact system administrator for more details");
                                            }
                                        }
                                        else
                                        {
                                            tracingService.Trace("{0}", "'ServiceConfigurations' does not have any record.");
                                            throw new InvalidPluginExecutionException("There are no active service configuration found for the selected missed collection. Please contact system administrator for more details");
                                        }
                                    }
                                    else
                                        tracingService.Trace("{0}", "'Target' entity does not have service configuration attribute. Leaving plug-in without applying business rules.");
                                }
                                else
                                    tracingService.Trace("{0}", "'Target' entity logical name is not " + ServiceRequestTableColumnNames.MissedBinTableName + ". Leaving plug-in without applying business rules.");
                            }
                            else
                                tracingService.Trace("{0}", "'Target' entity is null. Leaving plug-in without applying business rules.");
                        }
                        else
                            tracingService.Trace("{0}", "Context does not contain 'Target' input parameter. Leaving plug-in without applying business rules.");
                    }
                    else
                        tracingService.Trace("{0}", "Context message name is not 'Create'. Leaving plug-in without applying business rules.");
                }
                else
                    tracingService.Trace("{0}", "Plug-in execution pipeline stage is not 'PostOperation'. Leaving plug-in without applying business rules.");
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                tracingService.Trace("Fault exception occured executing PostMissedBin: {0}.", ex.ToString());
                throw new InvalidPluginExecutionException("Fault exception occured executing PostMissedBin: " + ex.Message + ".");
            }
            catch (Exception ex)
            {
                tracingService.Trace("An exception occured executing PostMissedBin: {0}.", ex.ToString());
                throw new InvalidPluginExecutionException("An exception occured executing PostMissedBin: " + ex.Message + ".");
            }

        }
    }
}
