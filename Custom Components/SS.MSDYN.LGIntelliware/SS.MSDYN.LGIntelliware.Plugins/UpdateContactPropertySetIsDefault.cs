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
        public UpdateContactPropertySetIsDefault() : base(typeof(UpdateContactPropertySetIsDefault))
        {
            RegisteredEvents.Add(new Tuple<int, string, string, Action<LocalPluginContext>>(PluginExecutionPipelineStage.PreOperation.GetHashCode(), PluginExecutionMessageName.UPDATE, ContactProperty.TableName, Execute));
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
                                    var contact = contactProperty.GetAttributeValue<EntityReference>(ContactProperty.Contact);
                                    var contactProperties = DataverseHelper.RetrieveContactProperties(service, ContactProperty.TableName, contact.Id, new ColumnSet(true));
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
                throw new InvalidPluginExecutionException("Fault exception occured executing UpdateContactPropertySetIsDefault: " + ex.Message + ".");
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An exception occured executing UpdateContactPropertySetIsDefault: " + ex.Message + ".");
            }

        }

    }
}
