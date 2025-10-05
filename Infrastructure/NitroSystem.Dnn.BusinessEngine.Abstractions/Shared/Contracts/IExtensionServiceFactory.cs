using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts
{
    public interface IExtensionServiceFactory
    {
        Task<IExtensionServiceViewModel> GetService(Guid serviceId);
        Task<IDictionary<string, object>> GetDependencyList(Guid scenarioId);
        Task<Guid> SaveService(IViewModel parentService, string extensionServiceJson);
    }
}
