using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.MSDYN.LGIntelliware.Plugins.Common
{
    public class ServiceRequestConfig
    {
        public string TableName { get; set; } = string.Empty;
        public string ContactColumnLogicalName { get; set; } = string.Empty;
        public string ReferenceNumberColumnLogicalName { get; set; } = string.Empty;
        public string ServiceConfigurationColumnLogicalName { get; set; } = string.Empty;
        public string IncidentIdColumnLogicalName { get; set; } = string.Empty;
    }
}
