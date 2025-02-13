using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SS.MSDYN.LGIntelliware.Plugins
{
    public class PostPlanningPermission : PluginBase
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public PostPlanningPermission() : base(typeof(PostPlanningPermission))
        {
            RegisteredEvents.Add(new Tuple<int, string, string, Action<LocalPluginContext>>(PluginExecutionPipelineStage.PostOperation.GetHashCode(), PluginExecutionMessageName.CREATE, ServiceRequestTableColumnNames.PlanningPermissionTableName, Execute));
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
                            // Check if entity reference is of type planning permission...
                            if (entity.LogicalName.Equals(ServiceRequestTableColumnNames.PlanningPermissionTableName))
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
                                                throw new InvalidPluginExecutionException("There are no active subject found for the selected planning permission. Please contact system administrator for more details");
                                            }
                                        }
                                        else
                                        {
                                            throw new InvalidPluginExecutionException("Selected planning permission does not have subject configuration. Please contact system administrator for more details");
                                        }
                                    }
                                    else
                                    {
                                        throw new InvalidPluginExecutionException("There are no active service configuration found for the selected planning permission. Please contact system administrator for more details");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new InvalidPluginExecutionException("Fault exception occured executing PostPlanningPermission: " + ex.Message + ".");
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An exception occured executing PostPlanningPermission: " + ex.Message + ".");
            }

        }
    }
}
