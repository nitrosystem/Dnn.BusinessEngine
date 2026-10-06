using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Extension;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Extension
{
    public interface IExtensionRepository
    {
        Task<Guid> AddAsync(ExtensionInfo objExtensionInfo);
        
        Task<string> GetVersionAsync(string extensionName);
        Task<ExtensionInfo> GetAsync(Guid id);
        Task<IReadOnlyList<ExtensionInfo>> GetsAsync(params string[] columns);

        Task<bool> UpdateAsync(ExtensionInfo objExtensionInfo, params string[] columns);

        Task<bool> DeleteAsync(Guid id);
    }
}
