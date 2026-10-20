using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Module;

namespace NitroSystem.Dnn.BusinessEngine.Repository.Module
{
    public class ModuleClientBehaviorRepository : IModuleClientBehaviorRepository
    {
        private readonly ISql _sql;

        public ModuleClientBehaviorRepository(ISql sql)
        {
            _sql = sql;
        }

        public async Task<Guid> AddAsync(ModuleClientBehaviorInfo objModuleClientBehaviorInfo)
        {
            return await _sql.InsertAsync<ModuleClientBehaviorInfo>(objModuleClientBehaviorInfo);
        }

        public async Task BulkInsertAsync(IReadOnlyList<ModuleClientBehaviorInfo> variables)
        {
            await _sql.BulkInsertAsync<ModuleClientBehaviorInfo>(variables);
        }

        public async Task<string> GetFieldClientBehaviorNameAsync(Guid id)
        {
            return await _sql.GetColumnValueAsync<ModuleClientBehaviorInfo, string>(id, "ModuleClientBehaviorName");
        }

        public async Task<IReadOnlyList<ModuleClientBehaviorInfo>> GetsAsync(Guid moduleId, params string[] columns)
        {
            return await _sql.GetByScopeAsync<ModuleClientBehaviorInfo>(moduleId, columns);
        }

        public async Task<IReadOnlyList<ModuleClientBehaviorInfo>> GetsByAncestorAsync(Guid scenarioId)
        {
            return await _sql.GetChildsByParentColumnAsync<ModuleInfo, ModuleClientBehaviorInfo>
             (
                 "ScenarioId", "ModuleId", scenarioId
             );
        }

        public async Task<bool> UpdateAsync(ModuleClientBehaviorInfo objModuleClientBehaviorInfo, params string[] columns)
        {
            return await _sql.UpdateAsync<ModuleClientBehaviorInfo>(objModuleClientBehaviorInfo, columns);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _sql.DeleteAsync<ModuleClientBehaviorInfo>(id);
        }
    }
}
