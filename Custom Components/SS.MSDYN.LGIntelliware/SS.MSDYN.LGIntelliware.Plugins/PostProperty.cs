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
        public PostProperty() : base(typeof(PostProperty))
        {
            RegisteredEvents.Add(new Tuple<int, string, string, Action<LocalPluginContext>>(PluginExecutionPipelineStage.PostOperation.GetHashCode(), PluginExecutionMessageName.CREATE, Property.TableName, Execute));
        }
        protected void Execute(LocalPluginContext localContext)
        {
            if (localContext == null) throw new ArgumentNullException(nameof(localContext));
            var context = localContext.PluginExecutionContext;
            //var tracingService = localContext.TracingService;
            var service = localContext.OrganizationService;
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
                                //var uprn = entity.GetAttributeValue<string>(ContactTableColumnNames.Uprn);
                                var propertyId = entity.Id;
                                var contact = entity.GetAttributeValue<string>(Property.Contact);
                                if (contact != null)
                                {
                                    DataverseHelper.CreatePropertyContact(service, new Guid(contact), propertyId, false);
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An exception occured executing PostProperty: " + ex.Message + ".");
            }

        }

    }
}
