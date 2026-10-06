using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Module;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;

namespace NitroSystem.Dnn.BusinessEngine.Repository.Module
{
    public class ModuleFieldRepository : IModuleFieldRepository
    {
        private readonly ICacheService _cacheService;
        private readonly ISql _sql;

        public ModuleFieldRepository(ISql sql, ICacheService cacheService)
        {
            _cacheService = cacheService;
            _sql = sql;
        }

        public async Task<Guid> AddAsync(ModuleFieldInfo objModuleFieldInfo)
        {
            return await _sql.InsertAsync<ModuleFieldInfo>(objModuleFieldInfo);
        }

        public async Task<string> GetFieldTypeAsync(Guid id)
        {
            return await _sql.GetColumnValueAsync<ModuleFieldTypeInfo, string>(id, "FieldType");
        }

        public async Task<ModuleFieldInfo> GetAsync(Guid id)
        {
            return await _sql.GetAsync<ModuleFieldInfo>(id);
        }

        public async Task<IReadOnlyList<ModuleFieldInfo>> GetsAsync(Guid moduleId, params string[] columns)
        {
            return await _sql.GetByScopeAsync<ModuleFieldInfo>(moduleId, columns);
        }

        public async Task<bool> UpdateAsync(ModuleFieldInfo objModuleFieldInfo, params string[] columns)
        {
            return await _sql.UpdateAsync<ModuleFieldInfo>(objModuleFieldInfo, columns);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _sql.DeleteAsync<ModuleFieldInfo>(id);
        }

        #region Stored Procedures

        public async Task SortFieldsAsync(Guid moduleId, string paneName, string fieldIds)
        {
            await _sql.ExecuteStoredProcedureAsync("dbo.BusinessEngine_Studio_SortModuleFields",
                new
                {
                    ModuleId = moduleId,
                    PaneName = paneName,
                    FieldIds = fieldIds
                }
            );

            var cacheKey = AttributeCache.Instance.GetCache<ModuleFieldInfo>().key;
            _cacheService.RemoveByPrefix(cacheKey);
        }

        #endregion
    }
}
