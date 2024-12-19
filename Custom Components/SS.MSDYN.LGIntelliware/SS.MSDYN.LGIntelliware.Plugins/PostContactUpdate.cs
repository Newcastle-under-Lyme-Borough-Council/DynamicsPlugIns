using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SS.MSDYN.LGIntelliware.Plugins
{
    public class PostContactUpdate : PluginBase
    {
        public PostContactUpdate() : base(typeof(PostContactUpdate))
        {
            RegisteredEvents.Add(new Tuple<int, string, string, Action<LocalPluginContext>>(PluginExecutionPipelineStage.PostOperation.GetHashCode(), PluginExecutionMessageName.UPDATE, ContactTableColumnNames.TableName, Execute));
        }
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
                    if (context.MessageName.Equals(PluginExecutionMessageName.UPDATE))
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
                                tracingService.Trace("{0}", "Going to check entity reference logical name.");
                                if (entity.LogicalName.Equals(ContactTableColumnNames.TableName))
                                {
                                    tracingService.Trace("{0}", "'Target' entity logical name: " + entity.LogicalName + ".");
                                    var contactId = entity.Id;
                                    var uprn = entity.GetAttributeValue<string>(ContactTableColumnNames.Uprn);
                                    tracingService.Trace("contactid " + contactId + " uprn" + uprn);
                                    //delete previous records 
                                    var retrieveContactProperties = DataverseHelper.RetrieveContactProperties(service, tracingService, ContactPropertyTableColumnNames.TableName, contactId, new ColumnSet(false));
                                    if (retrieveContactProperties != null && retrieveContactProperties.Entities.Count > 0)
                                    {
                                        DataverseHelper.DeleteContactProperties(service, tracingService, retrieveContactProperties);
                                    }
                                    var properties = DataverseHelper.RetrieveProperties(service, tracingService, PropertyTableColumnNames.TableName, uprn, new ColumnSet(false));
                                    if (properties != null && properties.Entities.Count > 0)
                                    {
                                        DataverseHelper.CreateContactProperty(service, tracingService, properties, contactId);
                                    }
                                    else
                                        tracingService.Trace("{0}", "'No properties against uprn." + uprn);
                                }
                                else
                                    tracingService.Trace("{0}", "'Target' entity logical name is not " + ContactTableColumnNames.TableName + ". Leaving plug-in without applying business rules.");
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
                tracingService.Trace("Fault exception occured executing PostContactUpdate: {0}.", ex.ToString());
                throw new InvalidPluginExecutionException("Fault exception occured executing PostContactUpdate: " + ex.Message + ".");
            }
            catch (Exception ex)
            {
                tracingService.Trace("An exception occured executing PostContactUpdate: {0}.", ex.ToString());
                throw new InvalidPluginExecutionException("An exception occured executing PostContactUpdate: " + ex.Message + ".");
            }

        }
    }
}
