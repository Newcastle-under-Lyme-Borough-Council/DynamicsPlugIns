using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.MSDYN.LGIntelliware.Plugins
{
    public sealed class PluginExecutionMessageName
    {
        public const string CREATE = "Create";
        public const string UPDATE = "Update";
    }

    public sealed class ImageEntityAlias
    {
        public const string PREIMAGEALIAS = "PreImageAlias";
    }

    public sealed class ContextInputParameters
    {
        public const string ENTITYMONIKER = "EntityMoniker";
        public const string TARGET = "Target";
        public const string STATE = "State";
    }

    public sealed class ServiceConfigurationSubjects
    {
        public const string MISSEDBIN = "Missed Bin";
        public const string PLANNINGPERMISSION = "Planning Permission";
        public const string TAXILICENCE = "Taxi Licence";
    }

    public sealed class ServiceRequestStatuses
    {
        public const string OPEN = "Open";
        public const string INPROGRESSBACKOFFICE = "In Progress / Back Office";
        public const string CLOSEDREJECTED = "Closed / Rejected";
        public const string CLOSEDCOMPLETED = "Closed / Completed";
    }
}
