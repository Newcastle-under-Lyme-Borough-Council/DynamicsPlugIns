using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using SS.MSDYN.LGIntelliware.Plugins.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SS.MSDYN.LGIntelliware.Plugins
{
    public class ServiceRequestHandler : PluginBase
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        protected ServiceRequestConfig Config = new ServiceRequestConfig();

        public ServiceRequestHandler(string unsecureConfiguration)
           : base(typeof(ServiceRequestHandler), unsecureConfiguration)
        {
            if (!string.IsNullOrWhiteSpace(unsecureConfiguration))
            {
                Config = JsonSerializer.Deserialize<ServiceRequestConfig>(unsecureConfiguration);
            }
            RegisterEvents();
        }
        //Registers the plugin to run after a missedbin record is created.
        public ServiceRequestHandler() : base(typeof(ServiceRequestHandler))
        {
            RegisterEvents();
        }

        public void RegisterEvents()
        {
            RegisteredEvents.Add(new Tuple<int, string, string, Action<LocalPluginContext>>(PluginExecutionPipelineStage.PostOperation.GetHashCode(), PluginExecutionMessageName.CREATE, Config.TableName, Execute));
        }

        /// <summary>
        /// Executes plug-in code in response to an event.
        /// </summary>
        /// <param name="localContext">Contains a local plug-in context.</param>
        protected void Execute(LocalPluginContext localContext)
        {
            localContext.TracingService.Trace("UnsecuredString - " + Config.IncidentIdColumnLogicalName);

            if (localContext == null)
            {
                throw new ArgumentNullException(nameof(localContext));
            }
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
                    
                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new InvalidPluginExecutionException("Fault exception occured executing ServiceRequestHandler: " + ex + ".");
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An exception occured executing ServiceRequestHandler: " + ex + ".");
            }

        }
    }
}