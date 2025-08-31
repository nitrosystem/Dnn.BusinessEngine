using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Common.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts
{
    public interface IExtensionServiceFactory
    {
        Task<IExtensionServiceViewModel> GetService(Guid serviceId);

        Task<IDictionary<string, object>> GetDependencyList(Guid scenarioId);

        Task<Guid> SaveService(IViewModel parentService, string extensionServiceJson);
    }
}
