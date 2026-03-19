using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.MSDYN.LGIntelliware.Plugins
{
    /// <summary>
    /// Enumeration for Event Execution Pipeline.
    /// </summary>
    public enum PluginExecutionPipelineStage
    {
        PreValidation = 10,
        PreOperation = 20,
        MainOperation = 30,
        PostOperation = 40
    }

    /// <summary>
    /// Enumeration for State Code.
    /// </summary>
    public enum StateCode
    {
        Active = 0,
        Inactive = 1
    }

    /// <summary>
    /// Enumeration for Status Code.
    /// </summary>
    public enum StatusCode
    {
        Active = 1,
        Inactive = 2
    }

    /// <summary>
    /// Enumeration for incident priority.
    /// </summary>
    public enum IncidentPriority
    {
       Normal = 2
    }

    /// <summary>
    /// Enumeration for incident type.
    /// </summary>
    public enum IncidentType
    {
        Request = 3
    }

    /// <summary>
    /// Enumeration for incident origin.
    /// </summary>
    public enum IncidentOrigin
    {
        Phone = 1,
        Email = 2,
        Web = 3,
        Portal = 4,

    }

    /// <summary>
    /// Enumeration for service request source type.
    /// </summary>
    public enum ServiceRequestSourceType
    {
        Phone = 1,
        Web = 2,
        Email = 3,
        Portal = 4,
    }

}
