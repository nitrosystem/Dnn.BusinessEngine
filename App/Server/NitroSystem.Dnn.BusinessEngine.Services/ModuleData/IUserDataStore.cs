using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DotNetNuke.Entities.Portals;

namespace NitroSystem.Dnn.BusinessEngine.App.Services.ModuleData
{
    public interface IUserDataStore
    {
        Task<ConcurrentDictionary<string, object>> GetOrCreateModuleDataAsync(string connectionId, Guid moduleId, PortalSettings portalSettings);
        ConcurrentDictionary<string, object> GetDataForClients(Guid moduleId, ConcurrentDictionary<string, object> moduleData);
        Task<ConcurrentDictionary<string, object>> UpdateModuleData(string connectionId, Guid moduleId, Dictionary<string, object> incomingData, PortalSettings portalSettings);
        void Ping(string connectionId);
        void CleanupOldConnections(TimeSpan timeout);
    }
}
