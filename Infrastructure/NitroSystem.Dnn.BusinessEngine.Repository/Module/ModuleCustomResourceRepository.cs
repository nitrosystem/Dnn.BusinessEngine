using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Module;

namespace NitroSystem.Dnn.BusinessEngine.Repository.Module
{
    public class ModuleCustomResourceRepository : IModuleCustomResourceRepository
    {
        private readonly ISql _sql;

        public ModuleCustomResourceRepository(ISql sql)
        {
            _sql = sql;
        }

        public async Task<Guid> AddAsync(ModuleCustomResourceInfo objModuleCustomResourceInfo)
        {
            return await _sql.InsertAsync<ModuleCustomResourceInfo>(objModuleCustomResourceInfo);
        }

        public async Task BulkInsertAsync(IReadOnlyList<ModuleCustomResourceInfo> resources)
        {
            await _sql.BulkInsertAsync<ModuleCustomResourceInfo>(resources);
        }

        public async Task<ModuleCustomResourceInfo> GetAsync(Guid id)
        {
            return await _sql.GetAsync<ModuleCustomResourceInfo>(id);
        }

        public async Task<IReadOnlyList<ModuleCustomResourceInfo>> GetsAsync(Guid moduleId, params string[] columns)
        {
            return await _sql.GetByScopeAsync<ModuleCustomResourceInfo>(moduleId, columns);
        }

        public async Task<IReadOnlyList<ModuleCustomResourceInfo>> GetsByAncestorAsync(Guid scenarioId)
        {
            return await _sql.GetChildsByParentColumnAsync<ModuleInfo, ModuleCustomResourceInfo>
             (
                 "ScenarioId", "ModuleId", scenarioId
             );
        }

        public async Task<bool> UpdateAsync(ModuleCustomResourceInfo objModuleCustomResourceInfo, params string[] columns)
        {
            return await _sql.UpdateAsync<ModuleCustomResourceInfo>(objModuleCustomResourceInfo, columns);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _sql.DeleteAsync<ModuleCustomResourceInfo>(id);
        }
    }
}
