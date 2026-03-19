using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.Extensions;

namespace SS.MSDYN.LGIntelliware.Plugins
{
    public class PostContact : PluginBase
    {
        //Registers the plugin to run after a contact record is created.
        public PostContact() : base(typeof(PostContact))
        {
            RegisteredEvents.Add(new Tuple<int, string, string, Action<LocalPluginContext>>(PluginExecutionPipelineStage.PostOperation.GetHashCode(), PluginExecutionMessageName.CREATE, Contact.TableName, Execute));
        }
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
                            var contactId = entity.Id;
                            var uprn = entity.GetAttributeValue<string>(Contact.Uprn);
                            if (uprn != null)
                            {
                                // Check if a property already exists for this uprn
                                var propertyId = DataverseHelper.CheckPropertiesExist(service, Property.TableName, uprn, new ColumnSet(false));

                                if (propertyId == Guid.Empty)
                                {
                                    propertyId = DataverseHelper.CreateProperty(service, entity, uprn);
                                }

                                if (propertyId != Guid.Empty)
                                {
                                    var updateProperty = DataverseHelper.UpdatePropertyFromContact(service,entity,uprn,propertyId);
                                    // Retrieve all contact properties linked to this contact
                                    var existingContactProperties = DataverseHelper.RetrieveContactProperties(service, ContactProperty.TableName, contactId, new ColumnSet(ContactProperty.Property,ContactProperty.ContactPropertyId));
                                    // Check if the property is already linked to the contact
                                    foreach (var item in existingContactProperties.Entities)
                                    {
                                        if (item.Attributes.Contains(ContactProperty.Property) && ((EntityReference)(item.Attributes[ContactProperty.Property])).Id == propertyId)
                                        {
                                            DataverseHelper.SetContactPropertyToDefault(service, item.Id);
                                            return;
                                        }
                                    }
                                    // If the property is not linked to the contact, create a new contactProperty record
                                    DataverseHelper.CreatePropertyContact(service, contactId, propertyId, true);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An exception occured executing PostContact: " + ex + ".");
            }

        }

    }
}
