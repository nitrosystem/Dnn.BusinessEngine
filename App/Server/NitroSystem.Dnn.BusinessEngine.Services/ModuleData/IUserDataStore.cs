using DotNetNuke.Entities.Portals;
using Newtonsoft.Json.Linq;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NitroSystem.Dnn.BusinessEngine.App.Services.Delegates.Delegates;

namespace NitroSystem.Dnn.BusinessEngine.App.Services.ModuleData
{
    public interface IUserDataStore
    {
        Task<ConcurrentDictionary<string, object>> GetOrCreateModuleDataAsync(string connectionId, Guid moduleId, PortalSettings portalSettings);
        ConcurrentDictionary<string, object> GetOrCreateModuleData(string connectionId, Guid moduleId, PortalSettings portalSettings);

        ConcurrentDictionary<string, object> GetDataForClients(Guid moduleId, ConcurrentDictionary<string, object> moduleData);

        Task<ConcurrentDictionary<string, object>> UpdateModuleDataAsync(string connectionId, Guid moduleId, Dictionary<string, object> incomingData, PortalSettings portalSettings);

        void Ping(string connectionId);

        void CleanupOldConnections(TimeSpan timeout);
    }
}
