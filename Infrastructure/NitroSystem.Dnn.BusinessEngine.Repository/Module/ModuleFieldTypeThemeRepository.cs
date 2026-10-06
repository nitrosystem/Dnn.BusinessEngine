using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Module;

namespace NitroSystem.Dnn.BusinessEngine.Repository.Module
{
    public class ModuleFieldTypeThemeRepository: IModuleFieldTypeThemeRepository
    {
        private readonly ISql _sql;

        public ModuleFieldTypeThemeRepository(ISql sql)
        {
            _sql = sql;
        }

        public async Task<Guid> AddAsync(ModuleFieldTypeThemeInfo objModuleFieldTypeThemeInfo)
        {
            return await _sql.InsertAsync<ModuleFieldTypeThemeInfo>(objModuleFieldTypeThemeInfo);
        }

        public async Task<ModuleFieldTypeThemeInfo> GetAsync(Guid id)
        {
            return await _sql.GetAsync<ModuleFieldTypeThemeInfo>(id);
        }

        public async Task<IReadOnlyList<ModuleFieldTypeThemeInfo>> GetsAsync(params string[] columns)
        {
            return await _sql.GetAllAsync<ModuleFieldTypeThemeInfo>(columns);
        }

        public async Task<bool> UpdateAsync(ModuleFieldTypeThemeInfo objModuleFieldTypeThemeInfo, params string[] columns)
        {
            return await _sql.UpdateAsync<ModuleFieldTypeThemeInfo>(objModuleFieldTypeThemeInfo, columns);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _sql.DeleteAsync<ModuleFieldTypeThemeInfo>(id);
        }
    }
}
