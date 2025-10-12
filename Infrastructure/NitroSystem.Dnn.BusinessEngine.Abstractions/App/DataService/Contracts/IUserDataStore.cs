using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataServices.Contracts
{
    public interface IUserDataStore
    {
        Task<ConcurrentDictionary<string, object>> GetOrCreateModuleDataAsync(string connectionId, Guid moduleId);
        ConcurrentDictionary<string, object> GetDataForClients(Guid moduleId, ConcurrentDictionary<string, object> moduleData);
        Task<ConcurrentDictionary<string, object>> UpdateModuleData(string connectionId, Guid moduleId, Dictionary<string, object> incomingData);
        void Ping(string connectionId);
        void CleanupOldConnections(TimeSpan timeout);
    }
}
