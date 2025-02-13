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
            RegisteredEvents.Add(new Tuple<int, string, string, Action<LocalPluginContext>>(PluginExecutionPipelineStage.PostOperation.GetHashCode(), PluginExecutionMessageName.UPDATE, Property.TableName, Execute));
        }
        protected void Execute(LocalPluginContext localContext)
        {
            if (localContext == null) throw new ArgumentNullException(nameof(localContext));
            var context = localContext.PluginExecutionContext;
            //var tracingService = localContext.TracingService;
            var service = localContext.OrganizationService;

            try
            {
                // Check if context message name is 'Update' ...
                if (context.MessageName.Equals(PluginExecutionMessageName.UPDATE))
                {
                    // Check if context has 'Target' input parameter...
                    if (context.InputParameters.ContainsKey(ContextInputParameters.TARGET) && context.InputParameters[ContextInputParameters.TARGET] != null)
                    {
                        var entity = (Entity)context.InputParameters[ContextInputParameters.TARGET];
                        if (entity != null)
                        {
                            if (entity.LogicalName.Equals(Property.TableName))
                            {
                                var propertyId = entity.Id;
                                var uprn = entity.GetAttributeValue<string>(Property.Uprn);
                                var contact = entity.GetAttributeValue<string>(Property.Contact);
                                //delete previous records 
                                var retrievePropertiesContact = DataverseHelper.RetrievePropertiesContact(service, ContactProperty.TableName, propertyId, new ColumnSet(false));
                                if (retrievePropertiesContact != null && retrievePropertiesContact.Entities.Count > 0)
                                {
                                    DataverseHelper.DeleteContactProperties(service, retrievePropertiesContact);
                                }
                                var contacts = DataverseHelper.RetrieveContacts(service, Contact.TableName, uprn, new ColumnSet(false));
                                if (contacts != null && contacts.Entities.Count > 0)
                                {
                                    DataverseHelper.AddContactProperty(service, contacts, propertyId);

                                }

                            }
                        }
                    }
                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new InvalidPluginExecutionException("Fault exception occured executing PostPropertyUpdate: " + ex.Message + ".");
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An exception occured executing PostPropertyUpdate: " + ex.Message + ".");
            }

        }
    }
}
