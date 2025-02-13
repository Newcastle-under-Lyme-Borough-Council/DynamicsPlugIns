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
            RegisteredEvents.Add(new Tuple<int, string, string, Action<LocalPluginContext>>(PluginExecutionPipelineStage.PostOperation.GetHashCode(), PluginExecutionMessageName.UPDATE, ContactTableColumnNames.TableName, Execute));
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
                            if (entity.LogicalName.Equals(ContactTableColumnNames.TableName))
                            {
                                var contactId = entity.Id;
                                var uprn = entity.GetAttributeValue<string>(ContactTableColumnNames.Uprn);
                                //delete previous records 
                                //var retrieveContactProperties = DataverseHelper.RetrieveContactProperties(service, tracingService, ContactPropertyTableColumnNames.TableName, contactId, new ColumnSet(false));
                                //if (retrieveContactProperties != null && retrieveContactProperties.Entities.Count > 0)
                                //{
                                //    DataverseHelper.DeleteContactProperties(service, tracingService, retrieveContactProperties);
                                //}
                                //var check = DataverseHelper.RetrieveCreatedProperty(service, tracingService, PropertyTableColumnNames.TableName, uprn,contactId, new ColumnSet(false));
                                var propertiesRecords = DataverseHelper.RetrievePropertiesContact(service, PropertyTableColumnNames.TableName, contactId, uprn, new ColumnSet(false));
                                var fetchpropertyId = DataverseHelper.CheckPropertiesContactExist(service, PropertyTableColumnNames.TableName, contactId, uprn, new ColumnSet(false));
                                if (fetchpropertyId != new Guid())
                                {
                                    //DataverseHelper.CreateContactProperty(service, tracingService, properties, contactId);
                                   // DataverseHelper.UpdatePropertiesIsDefault(service, propertiesRecords, contactId);
                                 //   DataverseHelper.SetPropertyToDefault(service, fetchpropertyId);

                                }
                                else
                                {
                                    var propertyId = DataverseHelper.CreateProperty(service, contactId, uprn);
                                    if (propertyId != new Guid())
                                    {
                                        DataverseHelper.CreatePropertyContact(service, contactId, propertyId);
                                        //DataverseHelper.UpdatePropertiesIsDefault(service, propertiesRecords, contactId);
                                   //     DataverseHelper.SetPropertyToDefault(service, propertyId);
                                    }

                                }
                            }
                        }
                    }
                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new InvalidPluginExecutionException("Fault exception occured executing PostContactUpdate: " + ex.Message + ".");
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An exception occured executing PostContactUpdate: " + ex.Message + ".");
            }

        }
    }
}
