using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System;
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
        //Registers the plugin to run after a contact record is updated.
        public PostContactUpdate() : base(typeof(PostContactUpdate))
        {
            RegisteredEvents.Add(new Tuple<int, string, string, Action<LocalPluginContext>>(PluginExecutionPipelineStage.PostOperation.GetHashCode(), PluginExecutionMessageName.UPDATE, Contact.TableName, Execute));
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
                // Check if context message name is 'Update' ...
                if (context.MessageName.Equals(PluginExecutionMessageName.UPDATE))
                {
                    // Check if context has 'Target' input parameter...
                    if (context.InputParameters.ContainsKey(ContextInputParameters.TARGET) && context.InputParameters[ContextInputParameters.TARGET] != null)
                    {
                        var entity = (Entity)context.InputParameters[ContextInputParameters.TARGET];
                        if (entity != null)
                        {
                            var contactId = entity.Id;
                            Entity postImage = null;
                            // Retrieve the postImage snapshot of the Contact record after update.
                            if (context.PostEntityImages != null &&
                            context.PostEntityImages.Contains("PostImage") &&
                        context.PostEntityImages["PostImage"] is Entity img)
                            {
                                postImage = img;
                                // Proceed with using postImage
                                var uprn = postImage.GetAttributeValue<string>(Contact.Uprn);
                                var propertyId = DataverseHelper.CheckPropertiesExist(service, Property.TableName, uprn, new ColumnSet(false));
                                if (propertyId == Guid.Empty)
                                {
                                    propertyId = DataverseHelper.CreateProperty(service, postImage, uprn);
                                }

                                // If a valid property exists, handle the contact property relationship
                                if (propertyId != Guid.Empty)
                                {
                                    var updateProperty = DataverseHelper.UpdatePropertyFromContact(service, postImage, uprn, propertyId);
                                    var existingContactProperties = DataverseHelper.RetrieveContactProperties(service, ContactProperty.TableName, contactId, new ColumnSet(ContactProperty.Property, ContactProperty.ContactPropertyId));
                                    foreach (var item in existingContactProperties.Entities)
                                    {

                                        if (item.Attributes.Contains(ContactProperty.Property) && ((EntityReference)(item.Attributes[ContactProperty.Property])).Id == propertyId)
                                        {

                                            DataverseHelper.SetContactPropertyToDefault(service, item.Id);
                                            return;
                                        }
                                    }
                                    // Create a new contact property relationship
                                    DataverseHelper.CreatePropertyContact(service, contactId, propertyId, true);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An exception occured executing PostContactUpdate: " + ex + ".");
            }

        }
    }
}
