using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Contracts
{
    public interface IUserDataStore
    {
        Task<ConcurrentDictionary<string, object>> GetOrCreateModuleDataAsync(string connectionId, Guid moduleId, string basePath);
        Task<ConcurrentDictionary<string, object>> UpdateModuleData(string connectionId, Guid moduleId, Dictionary<string, object> incomingData, string basePath);
        ConcurrentDictionary<string, object> GetDataForClients(string connectionId, Guid moduleId);
        void DisconnectUser(string connectionId, Guid moduleId);
    }
}
