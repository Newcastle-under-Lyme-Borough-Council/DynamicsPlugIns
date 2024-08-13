using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.ServiceModel;

using Microsoft.Xrm.Sdk;

namespace SS.MSDYN.LGIntelliware.Plugins
{
    /// <summary>
    /// Base class for all Plugins.
    /// </summary>    
    public class PluginBase : IPlugin
    {

        private Collection<Tuple<int, string, string, Action<LocalPluginContext>>> _registeredEvents;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginBase"/> class.
        /// </summary>
        internal PluginBase(Type childClassName) { ChildClassName = childClassName.ToString(); }

        /// <summary>
        /// Gets the List of events that the plug-in should fire for. Each List
        /// Item is a <see cref="System.Tuple"/> containing the Pipeline Stage, Message and (optionally) the Primary Entity. 
        /// In addition, the fourth parameter provide the delegate to invoke on a matching registration.
        /// </summary>
        protected Collection<Tuple<int, string, string, Action<LocalPluginContext>>> RegisteredEvents => _registeredEvents ?? (_registeredEvents = new Collection<Tuple<int, string, string, Action<LocalPluginContext>>>());

        /// <summary>
        /// Gets or sets the name of the child class.
        /// </summary>
        /// <value>The name of the child class.</value>
        protected string ChildClassName { get; }

        #region IPlugin Members

        /// <summary>
        /// Executes the plug-in.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <remarks>
        /// For improved performance, Microsoft Dynamics caches plug-in instances. 
        /// The plug-in's Execute method should be written to be stateless as the constructor 
        /// is not called for every invocation of the plug-in. Also, multiple system threads 
        /// could execute the plug-in at the same time. All per invocation state information 
        /// is stored in the context. This means that you should not use global variables in plug-ins.
        /// </remarks>
        public void Execute(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));

            // Construct the Local plug-in context.
            var localContext = new LocalPluginContext(serviceProvider);
            localContext.Trace(string.Format(CultureInfo.InvariantCulture, "Entered {0}.Execute()", ChildClassName));

            try
            {
                // Iterate over all of the expected registered events to ensure that the plugin has been invoked by an expected event
                // For any given plug-in event at an instance in time, we would expect at most 1 result to match.
                var entityAction =
                    (from a in RegisteredEvents
                     where (
                               a.Item1 == localContext.PluginExecutionContext.Stage &&
                               a.Item2 == localContext.PluginExecutionContext.MessageName &&
                               (string.IsNullOrWhiteSpace(a.Item3) || a.Item3 == localContext.PluginExecutionContext.PrimaryEntityName)
                           )
                     select a.Item4).FirstOrDefault();

                if (entityAction == null) return;
                localContext.Trace($"{ChildClassName} is firing for Entity: {localContext.PluginExecutionContext.PrimaryEntityName}, Message: {localContext.PluginExecutionContext.MessageName}");

                entityAction.Invoke(localContext);
            }
            catch (FaultException<OrganizationServiceFault> e)
            {
                localContext.Trace($"Exception: {e}");

                // Handle the exception.
                throw;
            }
            finally
            {
                localContext.Trace($"Exiting {ChildClassName}.Execute()");
            }
        }

        #endregion

        #region Nested type: LocalPluginContext

        protected class LocalPluginContext
        {
            // ReSharper disable once UnusedMember.Local
            private LocalPluginContext() { }

            internal LocalPluginContext(IServiceProvider serviceProvider)
            {
                if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));

                // Obtain the execution context service from the service provider.
                PluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

                // Obtain the tracing service from the service provider.
                TracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                // Obtain the Organization Service factory service from the service provider
                var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

                // Use the factory to generate the Organization Service.
                OrganizationService = factory.CreateOrganizationService(PluginExecutionContext.UserId);
            }

            internal IOrganizationService OrganizationService { get; }
            internal IPluginExecutionContext PluginExecutionContext { get; }
            internal ITracingService TracingService { get; }

            internal void Trace(string message)
            {
                if (string.IsNullOrWhiteSpace(message) || TracingService == null) return;

                TracingService.Trace(PluginExecutionContext == null
                    ? message
                    : $"{message}, Correlation Id: {PluginExecutionContext.CorrelationId}, Initiating User: {PluginExecutionContext.InitiatingUserId}, BU: {PluginExecutionContext.BusinessUnitId}, Depth: {PluginExecutionContext.Depth}");
            }
        }
        #endregion
    }
}