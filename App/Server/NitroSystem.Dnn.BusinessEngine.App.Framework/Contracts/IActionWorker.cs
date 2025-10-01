using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DotNetNuke.Entities.Portals;

namespace NitroSystem.Dnn.BusinessEngine.Framework.Contracts
{
    public interface IActionWorker
    {
        Task CallActions(ConcurrentDictionary<string, object> moduleData, Guid moduleId, Guid? fieldId, string eventName, PortalSettings portalSettings);
        Task CallActions(IEnumerable<Guid> actionIds, ConcurrentDictionary<string, object> moduleData, PortalSettings portalSettings);
    }
}
