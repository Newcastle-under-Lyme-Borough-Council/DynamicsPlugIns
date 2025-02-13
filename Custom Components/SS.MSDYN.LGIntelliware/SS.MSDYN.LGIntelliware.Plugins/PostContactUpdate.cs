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
            RegisteredEvents.Add(new Tuple<int, string, string, Action<LocalPluginContext>>(PluginExecutionPipelineStage.PostOperation.GetHashCode(), PluginExecutionMessageName.UPDATE, Contact.TableName, Execute));
        }
        protected void Execute(LocalPluginContext localContext)
        {
            if (localContext == null) throw new ArgumentNullException(nameof(localContext));
            var context = localContext.PluginExecutionContext;
            //var tracingService = localContext.TracingService;
            var service = localContext.OrganizationService;
            try
            {
                // Check if context message name is 'Create' ...
                if (context.MessageName.Equals(PluginExecutionMessageName.UPDATE))
                {
                    // Check if context has 'Target' input parameter...
                    if (context.InputParameters.ContainsKey(ContextInputParameters.TARGET) && context.InputParameters[ContextInputParameters.TARGET] != null)
                    {
                        var entity = (Entity)context.InputParameters[ContextInputParameters.TARGET];
                        if (entity != null)
                        {
                            var contactId = entity.Id;
                            var contact = DataverseHelper.RetrieveContact(service, Contact.TableName, contactId, new ColumnSet(true));
                            var uprn = entity.GetAttributeValue<string>(Contact.Uprn);
                            var propertyId = DataverseHelper.CheckPropertiesExist(service, Property.TableName, uprn, new ColumnSet(false));

                            if (propertyId == Guid.Empty)
                            {
                                propertyId = DataverseHelper.CreateProperty(service, contact, uprn);
                            }

                            if (propertyId != Guid.Empty)
                            {
                                var ExistingContactProperties = DataverseHelper.RetrieveContactProperties(service, ContactProperty.TableName, contactId, new ColumnSet(true));
                                foreach (var item in ExistingContactProperties.Entities)
                                {
             
                                    if (item.Attributes.Contains(ContactProperty.Property) && ((EntityReference)(item.Attributes[ContactProperty.Property])).Id == propertyId)
                                    {
                                        DataverseHelper.SetContactPropertyToDefault(service, item.Id);
                                        return;
                                    }
                                }
                                DataverseHelper.CreatePropertyContact(service, contactId, propertyId, true);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An exception occured executing PostContactUpdate: " + ex.Message + ".");
            }

        }
    }
}
