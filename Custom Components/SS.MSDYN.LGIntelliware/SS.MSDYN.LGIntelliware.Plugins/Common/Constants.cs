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
}
