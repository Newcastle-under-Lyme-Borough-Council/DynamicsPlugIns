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
            var service = localContext.OrganizationService;
            // Prevent infinite loops by limiting depth
            if (context.Depth > 2)
            {
                return;
            }
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
                            if (entity.LogicalName.Equals(ContactProperty.TableName))
                            {
                                var IsDefault = entity.GetAttributeValue<bool>(ContactProperty.IsDefault);
                                //Only proceed if this contact property is set as default
                                if (IsDefault == true)
                                {
                                    var contactProperty = DataverseHelper.RetrieveContactProperty(service, ContactProperty.TableName, entity.Id, new ColumnSet(ContactProperty.Contact, ContactProperty.Property));
                                    
                                    var contactId = contactProperty.GetAttributeValue<EntityReference>(ContactProperty.Contact);
                                    var contact = DataverseHelper.RetrieveContact(service, Contact.TableName, contactId.Id, new ColumnSet(Contact.Uprn, Contact.ContactId));
                                    
                                    var propertyId = contactProperty.GetAttributeValue<EntityReference>(ContactProperty.Property);
                                    var property = DataverseHelper.RetrieveProperty(service, Property.TableName, propertyId.Id, new ColumnSet(Property.Uprn, Property.Addresscs, Property.County, Property.Addressoscs, Property.Localityname, Property.Streetname, Property.TownName, Property.PostCode, Property.Posttown, Property.Region, Property.Latitude, Property.Longitude));

                                    // Update Contact with property data if uprn do not match
                                    if (property.Attributes.Contains(Property.Uprn) && contact.Attributes.Contains(Contact.Uprn)
                                        && property.Attributes[Property.Uprn] != contact.Attributes[Contact.Uprn])
                                    {
                                        DataverseHelper.UpdateContactwithPropertyData(service, Contact.TableName, contact.Id, property);
                                    }
                                    // Remove IsDefault flag from all other contact property records for this contact
                                    var contactProperties = DataverseHelper.RetrieveContactProperties(service, ContactProperty.TableName, contactId.Id, new ColumnSet(ContactProperty.IsDefault,ContactProperty.ContactPropertyId));
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
                throw new InvalidPluginExecutionException("An exception occured executing UpdateContactPropertySetIsDefault: " + ex + ".");
            }

        }

    }
}
