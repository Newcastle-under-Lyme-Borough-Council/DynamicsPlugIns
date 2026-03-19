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
    public class CreateContactPropertySetIsDefault : PluginBase
    {
        // Registers the plugin to run after a contactProperty record is created.
        public CreateContactPropertySetIsDefault() : base(typeof(CreateContactPropertySetIsDefault))
        {
            RegisteredEvents.Add(new Tuple<int, string, string, Action<LocalPluginContext>>(PluginExecutionPipelineStage.PostOperation.GetHashCode(), PluginExecutionMessageName.CREATE, ContactProperty.TableName, Execute));
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
                // Check if context message name is 'Create' ...
                if (context.MessageName.Equals(PluginExecutionMessageName.CREATE))
                {
                    // Check if context has 'Target' input parameter...
                    if (context.InputParameters.ContainsKey(ContextInputParameters.TARGET) && context.InputParameters[ContextInputParameters.TARGET] != null)
                    {
                        var entity = (Entity)context.InputParameters[ContextInputParameters.TARGET];
                        if (entity != null)
                        {
                            if (entity.LogicalName.Equals(ContactProperty.TableName))
                            {
                                var isDefault = entity.GetAttributeValue<bool>(ContactProperty.IsDefault);
                                if (isDefault == true)
                                {
                                    var contact = entity.GetAttributeValue<EntityReference>(ContactProperty.Contact);
                                    var contactProperties = DataverseHelper.RetrieveContactProperties(service, ContactProperty.TableName, contact.Id, new ColumnSet(ContactProperty.IsDefault, ContactProperty.ContactPropertyId));
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
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new InvalidPluginExecutionException("Fault exception occured executing CreateContactPropertySetIsDefault: " + ex + ".");
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An exception occured executing CreateContactPropertySetIsDefault: " + ex + ".");
            }

        }

    }
}
