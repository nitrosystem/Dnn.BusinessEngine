using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Module;

namespace NitroSystem.Dnn.BusinessEngine.Repository.Module
{
    public class ModuleFieldDataSourceRepository : IModuleFieldDataSourceRepository
    {
        private readonly ISql _sql;

        public ModuleFieldDataSourceRepository(ISql sql)
        {
            _sql = sql;
        }

        public async Task<Guid> AddAsync(ModuleFieldDataSourceInfo objModuleFieldDataSourceInfo)
        {
            return await _sql.InsertAsync<ModuleFieldDataSourceInfo>(objModuleFieldDataSourceInfo);
        }

        public async Task<ModuleFieldDataSourceInfo> GetAsync(Guid id)
        {
            return await _sql.GetAsync<ModuleFieldDataSourceInfo>(id);
        }

        public async Task<IReadOnlyList<ModuleFieldDataSourceInfo>> GetsByAncestorAsync(Guid moduleId)
        {
            return await _sql.GetChildsByParentColumnAsync<ModuleFieldInfo, ModuleFieldDataSourceInfo>
             (
                 "ModuleId", "FieldId", moduleId
             );
        }

        public async Task<IReadOnlyList<ModuleFieldDataSourceInfo>> GetsAsync(Guid fieldId, params string[] columns)
        {
            return await _sql.GetByScopeAsync<ModuleFieldDataSourceInfo>(fieldId, columns);
        }

        public async Task<bool> UpdateAsync(ModuleFieldDataSourceInfo objModuleFieldDataSourceInfo, params string[] columns)
        {
            return await _sql.UpdateAsync<ModuleFieldDataSourceInfo>(objModuleFieldDataSourceInfo, columns);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _sql.DeleteAsync<ModuleFieldDataSourceInfo>(id);
        }

        public async Task<bool> DeletesAsync(Guid fieldId)
        {
            return await _sql.DeleteByScopeAsync<ModuleFieldDataSourceInfo>(fieldId);
        }
    }
}
