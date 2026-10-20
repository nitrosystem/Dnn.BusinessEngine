using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Module;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Module
{
    public interface IModuleCustomResourceRepository
    {
        Task<Guid> AddAsync(ModuleCustomResourceInfo objModuleCustomResourceInfo);
        Task BulkInsertAsync(IReadOnlyList<ModuleCustomResourceInfo> resources);

        Task<ModuleCustomResourceInfo> GetAsync(Guid id);
        Task<IReadOnlyList<ModuleCustomResourceInfo>> GetsAsync(Guid moduleId, params string[] columns);
        Task<IReadOnlyList<ModuleCustomResourceInfo>> GetsByAncestorAsync(Guid scenarioId);

        Task<bool> UpdateAsync(ModuleCustomResourceInfo objModuleCustomResourceInfo, params string[] columns);

        Task<bool> DeleteAsync(Guid id);
    }
}
