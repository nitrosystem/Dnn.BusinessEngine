using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Module;

namespace NitroSystem.Dnn.BusinessEngine.Repository.Module
{
    public class ModuleFieldTypeRepository : IModuleFieldTypeRepository
    {
        private readonly ISql _sql;

        public ModuleFieldTypeRepository(ISql sql)
        {
            _sql = sql;
        }

        public async Task<Guid> AddAsync(ModuleFieldTypeInfo objModuleFieldTypeInfo)
        {
            return await _sql.InsertAsync<ModuleFieldTypeInfo>(objModuleFieldTypeInfo);
        }

        public async Task<string> GetIconAsync(string fieldType)
        {
            return await _sql.GetColumnValueAsync<ModuleFieldTypeInfo, string>("Icon", "FieldType", fieldType);
        }

        public async Task<string> GetGeneratePanesBusinessControllerClassAsync(string fieldType)
        {
            return await _sql.GetColumnValueAsync<ModuleFieldTypeInfo, string>("GeneratePanesBusinessControllerClass", "FieldType", fieldType);
        }

        public async Task<ModuleFieldTypeInfo> GetAsync(Guid id)
        {
            return await _sql.GetAsync<ModuleFieldTypeInfo>(id);
        }

        public async Task<IReadOnlyList<ModuleFieldTypeInfo>> GetsAsync(params string[] columns)
        {
            return await _sql.GetAllAsync<ModuleFieldTypeInfo>(columns);
        }

        public async Task<bool> UpdateAsync(ModuleFieldTypeInfo objModuleFieldTypeInfo, params string[] columns)
        {
            return await _sql.UpdateAsync<ModuleFieldTypeInfo>(objModuleFieldTypeInfo, columns);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _sql.DeleteAsync<ModuleFieldTypeInfo>(id);
        }

        public async Task<bool> HasFieldTypeAsync()
        {
            return await _sql.ExistsAsync<ModuleFieldTypeInfo>();
        }
    }
}
