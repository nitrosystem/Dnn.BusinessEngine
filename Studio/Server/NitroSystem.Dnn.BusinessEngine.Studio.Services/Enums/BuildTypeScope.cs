using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Enums
{
    /// <summary>
    /// Represents the scope level for a build operation, 
    /// specifying how broadly the build process should apply.
    /// </summary>
    public enum BuildScope
    {
        /// <summary>
        /// Build only the specified module without affecting any related modules.
        /// </summary>
        OnlyOneModule = 0,

        /// <summary>
        /// Build all modules that belong to the same scenario.
        /// Ideal for full scenario-wide deployments.
        /// </summary>
        AllScenarioModules = 2
    }
}
