using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Module;

namespace NitroSystem.Dnn.BusinessEngine.Repository.Module
{
    public class ModuleFieldTypeTemplateRepository : IModuleFieldTypeTemplateRepository
    {
        private readonly ISql _sql;

        public ModuleFieldTypeTemplateRepository(ISql sql)
        {
            _sql = sql;
        }

        public async Task<Guid> AddAsync(ModuleFieldTypeTemplateInfo objModuleFieldTypeTemplateInfo)
        {
            return await _sql.InsertAsync<ModuleFieldTypeTemplateInfo>(objModuleFieldTypeTemplateInfo);
        }

        public async Task<ModuleFieldTypeTemplateInfo> GetAsync(Guid id)
        {
            return await _sql.GetAsync<ModuleFieldTypeTemplateInfo>(id);
        }

        public async Task<IReadOnlyList<ModuleFieldTypeTemplateInfo>> GetsAsync(params string[] columns)
        {
            return await _sql.GetAllAsync<ModuleFieldTypeTemplateInfo>(columns);
        }

        public async Task<bool> UpdateAsync(ModuleFieldTypeTemplateInfo objModuleFieldTypeTemplateInfo, params string[] columns)
        {
            return await _sql.UpdateAsync<ModuleFieldTypeTemplateInfo>(objModuleFieldTypeTemplateInfo, columns);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _sql.DeleteAsync<ModuleFieldTypeTemplateInfo>(id);
        }
    }
}
