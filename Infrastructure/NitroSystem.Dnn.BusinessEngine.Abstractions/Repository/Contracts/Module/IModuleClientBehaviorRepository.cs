using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Module;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Module
{
    public interface IModuleClientBehaviorRepository
    {
        Task<Guid> AddAsync(ModuleClientBehaviorInfo objModuleClientBehaviorInfo);

        Task BulkInsertAsync(IReadOnlyList<ModuleClientBehaviorInfo> variables);

        Task<string> GetFieldClientBehaviorNameAsync(Guid id);

        Task<IReadOnlyList<ModuleClientBehaviorInfo>> GetsAsync(Guid moduleId, params string[] columns);

        Task<IReadOnlyList<ModuleClientBehaviorInfo>> GetsByAncestorAsync(Guid scenarioId);

        Task<bool> UpdateAsync(ModuleClientBehaviorInfo objModuleClientBehaviorInfo, params string[] columns);

        Task<bool> DeleteAsync(Guid id);
    }
}
