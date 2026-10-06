using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Service;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts
{
    public interface IExtensionServiceFactory
    {
        Task<IExtensionServiceViewModel> GetService(Guid serviceId);
        Task<IDictionary<string, object>> GetDependencyList(Guid scenarioId);
        Task<Guid> SaveService(ServiceViewModel parentService, string extensionServiceJson);
    }
}
