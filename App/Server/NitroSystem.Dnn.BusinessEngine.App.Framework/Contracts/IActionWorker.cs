using DotNetNuke.Abstractions.Portals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Framework.Contracts
{
    public interface IActionWorker
    {
        Task CallActions(IModuleData moduleData, Guid moduleId, Guid? fieldId, string eventName, IPortalSettings portalSettings);

        Task CallActions(IEnumerable<Guid> actionIds, IModuleData moduleData, IPortalSettings portalSettings);
    }
}
