using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Module;

namespace NitroSystem.Dnn.BusinessEngine.Repository.Module
{
    public class ModuleFieldSettingRepository : IModuleFieldSettingRepository
    {
        private readonly ISql _sql;

        public ModuleFieldSettingRepository(ISql sql)
        {
            _sql = sql;
        }

        public async Task<Guid> AddAsync(ModuleFieldSettingInfo objModuleFieldSettingInfo)
        {
            return await _sql.InsertAsync<ModuleFieldSettingInfo>(objModuleFieldSettingInfo);
        }

        public async Task<ModuleFieldSettingInfo> GetAsync(Guid id)
        {
            return await _sql.GetAsync<ModuleFieldSettingInfo>(id);
        }

        public async Task<IReadOnlyList<ModuleFieldSettingInfo>> GetsByAncestorAsync(Guid moduleId)
        {
            return await _sql.GetChildsByParentColumnAsync<ModuleFieldInfo, ModuleFieldSettingInfo>
             (
                 "ModuleId", "FieldId", moduleId
             );
        }

        public async Task<IReadOnlyList<ModuleFieldSettingInfo>> GetsAsync(Guid fieldId, params string[] columns)
        {
            return await _sql.GetByScopeAsync<ModuleFieldSettingInfo>(fieldId, columns);
        }

        public async Task<bool> UpdateAsync(ModuleFieldSettingInfo objModuleFieldSettingInfo, params string[] columns)
        {
            return await _sql.UpdateAsync<ModuleFieldSettingInfo>(objModuleFieldSettingInfo, columns);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _sql.DeleteAsync<ModuleFieldSettingInfo>(id);
        }

        public async Task<bool> DeletesAsync(Guid fieldId)
        {
            return await _sql.DeleteByScopeAsync<ModuleFieldSettingInfo>(fieldId);
        }
    }
}
