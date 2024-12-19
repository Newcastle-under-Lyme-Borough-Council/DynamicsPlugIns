using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SS.MSDYN.LGIntelliware.Plugins
{
    public class PostPropertyUpdate : PluginBase
    {
        public PostPropertyUpdate() : base(typeof(PostPropertyUpdate))
        {
            RegisteredEvents.Add(new Tuple<int, string, string, Action<LocalPluginContext>>(PluginExecutionPipelineStage.PostOperation.GetHashCode(), PluginExecutionMessageName.UPDATE, PropertyTableColumnNames.TableName, Execute));
        }
        protected void Execute(LocalPluginContext localContext)
        {
            if (localContext == null) throw new ArgumentNullException(nameof(localContext));
            var context = localContext.PluginExecutionContext;
            var tracingService = localContext.TracingService;
            var service = localContext.OrganizationService;

            try
            {
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
                                if (entity.LogicalName.Equals(PropertyTableColumnNames.TableName))
                                {
                                    tracingService.Trace("{0}", "'Target' entity logical name: " + entity.LogicalName + ".");
                                    var propertyId = entity.Id;
                                    var uprn = entity.GetAttributeValue<string>(PropertyTableColumnNames.Uprn);
                                    //delete previous records 
                                    var retrievePropertiesContact = DataverseHelper.RetrievePropertiesContact(service, tracingService, ContactPropertyTableColumnNames.TableName, propertyId, new ColumnSet(false));
                                    if (retrievePropertiesContact != null && retrievePropertiesContact.Entities.Count > 0)
                                    {
                                        DataverseHelper.DeleteContactProperties(service, tracingService, retrievePropertiesContact);
                                    }
                                    var contacts = DataverseHelper.RetrieveContacts(service, tracingService, ContactTableColumnNames.TableName, uprn, new ColumnSet(false));
                                    if (contacts != null && contacts.Entities.Count > 0)
                                    {
                                        DataverseHelper.AddContactProperty(service, tracingService, contacts, propertyId);

                                    }
                                    else
                                        tracingService.Trace("{0}", "'No contacts against uprn." + uprn);
                                }
                                else
                                    tracingService.Trace("{0}", "'Target' entity logical name is not " + PropertyTableColumnNames.TableName + ". Leaving plug-in without applying business rules.");
                            }
                            else
                                tracingService.Trace("{0}", "'Target' entity is null. Leaving plug-in without applying business rules.");
                        }
                        else
                            tracingService.Trace("{0}", "Context does not contain 'Target' input parameter. Leaving plug-in without applying business rules.");
                    }
                    else
                        tracingService.Trace("{0}", "Context message name is not 'Update'. Leaving plug-in without applying business rules.");
                }
                else
                    tracingService.Trace("{0}", "Plug-in execution pipeline stage is not 'PostOperation'. Leaving plug-in without applying business rules.");
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                tracingService.Trace("Fault exception occured executing PostPropertyUpdate: {0}.", ex.ToString());
                throw new InvalidPluginExecutionException("Fault exception occured executing PostPropertyUpdate: " + ex.Message + ".");
            }
            catch (Exception ex)
            {
                tracingService.Trace("An exception occured executing PostPropertyUpdate: {0}.", ex.ToString());
                throw new InvalidPluginExecutionException("An exception occured executing PostPropertyUpdate: " + ex.Message + ".");
            }

        }
    }
}
