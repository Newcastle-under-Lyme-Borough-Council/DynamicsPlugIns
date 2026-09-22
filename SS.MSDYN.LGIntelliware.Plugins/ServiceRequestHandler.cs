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
        public ServiceRequestHandler(string unsecureConfiguration)
          : base(typeof(ServiceRequestHandler), unsecureConfiguration)
        {// Add constructor logic here

        }
        protected void Execute(LocalPluginContext localContext)
        {

        }
    }
}