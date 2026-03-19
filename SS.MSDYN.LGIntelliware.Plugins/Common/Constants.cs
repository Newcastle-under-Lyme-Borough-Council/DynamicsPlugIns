using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.MSDYN.LGIntelliware.Plugins
{
    //Defines supported plugin execution message names (operations)
    public sealed class PluginExecutionMessageName
    {
        public const string CREATE = "Create";
        public const string UPDATE = "Update";
    }

    //Defines the alias names for image entities used in plugin registration
    public sealed class ImageEntityAlias
    {
        public const string PREIMAGEALIAS = "PreImageAlias";
    }

    //Defines keys used to access input parameters in the plugin execution context
    public sealed class ContextInputParameters
    {
        public const string ENTITYMONIKER = "EntityMoniker";
        public const string TARGET = "Target";
        public const string STATE = "State";
    }
}
