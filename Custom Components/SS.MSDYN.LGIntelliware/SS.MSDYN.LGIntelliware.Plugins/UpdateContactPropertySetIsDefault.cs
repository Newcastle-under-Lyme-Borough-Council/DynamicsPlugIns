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
    public class UpdateContactPropertySetIsDefault : PluginBase
    {
        //Registers the plugin to run after a contact property  record is updated.
        public UpdateContactPropertySetIsDefault() : base(typeof(UpdateContactPropertySetIsDefault))
        {
            RegisteredEvents.Add(new Tuple<int, string, string, Action<LocalPluginContext>>(PluginExecutionPipelineStage.PostOperation.GetHashCode(), PluginExecutionMessageName.UPDATE, ContactProperty.TableName, Execute));
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
                            if (entity.LogicalName.Equals(ContactProperty.TableName))
                            {
                                var IsDefault = entity.GetAttributeValue<bool>(ContactProperty.IsDefault);
                                if (IsDefault == true)
                                {
                                    var contactProperty = DataverseHelper.RetrieveContactProperty(service, ContactProperty.TableName, entity.Id, new ColumnSet(true));
                                    
                                    var ContactID = contactProperty.GetAttributeValue<EntityReference>(ContactProperty.Contact);
                                    var contact = DataverseHelper.RetrieveContact(service, Contact.TableName, ContactID.Id, new ColumnSet(true));
                                    
                                    var PropertyID = contactProperty.GetAttributeValue<EntityReference>(ContactProperty.Property);
                                    var property = DataverseHelper.RetrieveProperty(service, Property.TableName, PropertyID.Id, new ColumnSet(true));

                                    // Update Contact with property data if uprn do not match
                                    if (property.Attributes.Contains(Property.Uprn) && contact.Attributes.Contains(Contact.Uprn)
                                        && property.Attributes[Property.Uprn] != contact.Attributes[Contact.Uprn])
                                    {
                                        DataverseHelper.UpdateContactwithPropertyData(service, Contact.TableName, contact.Id, property);
                                    }
                                    
                                    var contactProperties = DataverseHelper.RetrieveContactProperties(service, ContactProperty.TableName, ContactID.Id, new ColumnSet(true));
                                    foreach (var cp in contactProperties.Entities)
                                    {
                                        if (cp.Contains(ContactProperty.IsDefault) && cp.Id != entity.Id)
                                        {
                                            if ((bool)(cp[ContactProperty.IsDefault]) == true)
                                            {
                                                DataverseHelper.RemoveOtherContactPropertyfromDefault(service, cp.Id);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An exception occured executing UpdateContactPropertySetIsDefault: " + ex.Message + ".");
            }

        }

    }
}
