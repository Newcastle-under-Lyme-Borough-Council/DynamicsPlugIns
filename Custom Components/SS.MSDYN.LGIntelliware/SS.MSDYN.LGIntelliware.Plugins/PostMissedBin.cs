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
            //var tracingService = localContext.TracingService;
            var service = localContext.OrganizationService;

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
                            if (entity.LogicalName.Equals(ServiceRequestTableColumnNames.MissedBinTableName))
                            {
                                if (entity.Attributes.ContainsKey(ServiceRequestTableColumnNames.ServiceConfiguration) && entity.Attributes[ServiceRequestTableColumnNames.ServiceConfiguration] != null)
                                {
                                    var serviceConfigurationId = entity.GetAttributeValue<EntityReference>(ServiceRequestTableColumnNames.ServiceConfiguration).Id;
                                    var serviceConfigurations = DataverseHelper.RetrieveServiceConfiguration(service, ServiceConfigurationTableColumnNames.TableName, serviceConfigurationId, new ColumnSet(ServiceConfigurationTableColumnNames.Subject, ServiceConfigurationTableColumnNames.Name));
                                    if (serviceConfigurations != null && serviceConfigurations.Entities != null && serviceConfigurations.Entities.Count > 0)
                                    {
                                        var serviceConfiguration = serviceConfigurations.Entities[0];
                                        if (serviceConfiguration.Attributes.ContainsKey(ServiceConfigurationTableColumnNames.Subject) && serviceConfiguration.Attributes[ServiceConfigurationTableColumnNames.Subject] != null)
                                        {
                                            var title = serviceConfiguration.GetAttributeValue<string>(ServiceConfigurationTableColumnNames.Subject);
                                            var subjects = DataverseHelper.RetrieveSubject(service, SubjectTableColumnNames.TableName, title, new ColumnSet(SubjectTableColumnNames.Title));
                                            if (subjects != null && subjects.Entities != null && subjects.Entities.Count > 0)
                                            {
                                                var subject = subjects.Entities[0];
                                                var incidentId = DataverseHelper.CreateIncident(service, entity, serviceConfiguration, subject);

                                                Entity entityToUpdate = new Entity(entity.LogicalName)
                                                {
                                                    Id = entity.Id
                                                };
                                                entityToUpdate.Attributes.Add(ServiceRequestTableColumnNames.Case, new EntityReference(IncidentTableColumnNames.TableName, incidentId));
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
                throw new InvalidPluginExecutionException("Fault exception occured executing PostMissedBin: " + ex.Message + ".");
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An exception occured executing PostMissedBin: " + ex.Message + ".");
            }

        }
    }
}
