using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Module;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Module
{
    public interface IModuleFieldDataSourceRepository
    {

        Task<Guid> AddAsync(ModuleFieldDataSourceInfo objModuleFieldDataSourceInfo);

        Task<ModuleFieldDataSourceInfo> GetAsync(Guid id);
        Task<IReadOnlyList<ModuleFieldDataSourceInfo>> GetsByAncestorAsync(Guid moduleId);
        Task<IReadOnlyList<ModuleFieldDataSourceInfo>> GetsAsync(Guid fieldId, params string[] columns);

        Task<bool> UpdateAsync(ModuleFieldDataSourceInfo objModuleFieldDataSourceInfo, params string[] columns);

        Task<bool> DeleteAsync(Guid id);
        Task<bool> DeletesAsync(Guid fieldId);
    }
}
