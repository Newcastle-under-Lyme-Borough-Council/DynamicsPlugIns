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
    public class PostProperty : PluginBase
    {  
        //Registers the plugin to run after a property record is created.
        public PostProperty() : base(typeof(PostProperty))
        {
            RegisteredEvents.Add(new Tuple<int, string, string, Action<LocalPluginContext>>(PluginExecutionPipelineStage.PostOperation.GetHashCode(), PluginExecutionMessageName.CREATE, Property.TableName, Execute));
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
                            if (entity.LogicalName.Equals(Property.TableName))
                            {
                                var entityId = entity.Id;
                                var uprn = entity.GetAttributeValue<string>(Property.Uprn);
                                var contactId = entity.GetAttributeValue<string>(Property.Contact);
                                //Check if a property with the same uprn already exists
                                var propertyId = DataverseHelper.CheckPropertiesExist(service, Property.TableName, uprn, new ColumnSet(false));

                                // If the newly created record is duplicate, delete it
                                if (entityId != propertyId)
                                {
                                    DataverseHelper.DeleteProperty(service, Property.TableName, entityId);
                                }
                                // Update existing property with new details
                                if (propertyId != Guid.Empty)
                                {
                                    Entity postImage = new Entity();
                                    if (context.PostEntityImages.Contains("PostTarget"))
                                    {
                                        postImage = context.PostEntityImages["PostTarget"];
                                        var updateProperty = DataverseHelper.UpdatePropertyDetails(service, postImage, uprn, propertyId);
                                    }
                                }

                                if (contactId != null)
                                {
                                    var existingContactProperties = DataverseHelper.RetrieveContactProperties(service, ContactProperty.TableName, new Guid(contactId), new ColumnSet(ContactProperty.Property, ContactProperty.Contact));
                                    // Check if relationship already exists
                                    foreach (var item in existingContactProperties.Entities)
                                    {
                                        if (item.Attributes.Contains(ContactProperty.Property) && ((EntityReference)(item.Attributes[ContactProperty.Property])).Id == propertyId
                                            && item.Attributes.Contains(ContactProperty.Contact) && ((EntityReference)(item.Attributes[ContactProperty.Contact])).Id == new Guid(contactId)
                                            )
                                        {
                                            return;
                                        }  
                                    }
                                    //Create a new contact property relationship if it does not exist
                                    DataverseHelper.CreatePropertyContact(service, new Guid(contactId), propertyId, false);
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An exception occured executing PostProperty: " + ex + ".");
            }

        }

    }
}
