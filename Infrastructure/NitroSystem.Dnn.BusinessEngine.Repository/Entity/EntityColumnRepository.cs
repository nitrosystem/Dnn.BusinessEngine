using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Entity;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Entity;

namespace NitroSystem.Dnn.BusinessEngine.Repository.Entity
{
    public class EntityColumnRepository: IEntityColumnRepository
    {
        private readonly ISql _sql;

        public EntityColumnRepository(ISql sql)
        {
            _sql = sql;
        }

        public async Task<Guid> AddAsync(EntityColumnInfo objEntityColumnInfo)
        {
            return await _sql.InsertAsync<EntityColumnInfo>(objEntityColumnInfo);
        }

        public async Task<EntityColumnInfo> GetAsync(Guid id)
        {
            return await _sql.GetAsync<EntityColumnInfo>(id);
        }

        public async Task<IReadOnlyList<EntityColumnInfo>> GetsAsync(Guid entityId, params string[] columns)
        {
            return await _sql.GetByScopeAsync<EntityColumnInfo>(entityId, columns);
        }

        public async Task<bool> UpdateAsync(EntityColumnInfo objEntityColumnInfo, params string[] columns)
        {
            return await _sql.UpdateAsync<EntityColumnInfo>(objEntityColumnInfo, columns);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _sql.DeleteAsync<EntityColumnInfo>(id);
        }

        public async Task<bool> DeletesAsync(Guid entityId)
        {
            return await _sql.DeleteByScopeAsync<EntityColumnInfo>(entityId);
        }

        #region Store Procedures

        public async Task<IReadOnlyList<EntityColumnInfo>> GetsAsync(Guid entityId)
        {
            return await _sql.ExecuteStoredProcedureAsListAsync<EntityColumnInfo>(
                "dbo.BusinessEngine_Studio_GetEntityColumns", "BE_Entities_Columns_GetEntityColumns",
            new
            {
                EntityId = entityId
            });
        }

        #endregion
    }
}
