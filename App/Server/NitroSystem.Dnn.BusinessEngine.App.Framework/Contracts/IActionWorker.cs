using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Entities.Portals;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Framework.Contracts
{
    public interface IActionWorker
    {
        Task CallActionsAsync(ConcurrentDictionary<string, object> moduleData, Guid moduleId, Guid? fieldId, string eventName, PortalSettings portalSettings);
        void CallActions(ConcurrentDictionary<string, object> moduleData, Guid moduleId, Guid? fieldId, string eventName, PortalSettings portalSettings);

        Task CallActionsAsync(IEnumerable<Guid> actionIds, ConcurrentDictionary<string, object> moduleData, PortalSettings portalSettings);
        void CallActions(IEnumerable<Guid> actionIds, ConcurrentDictionary<string, object> moduleData, PortalSettings portalSettings);
    }
}
