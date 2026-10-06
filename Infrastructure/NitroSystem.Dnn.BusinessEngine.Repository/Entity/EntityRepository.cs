using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Entity;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Entity;

namespace NitroSystem.Dnn.BusinessEngine.Repository.Entity
{
    public class EntityRepository : IEntityRepository
    {
        private readonly ISql _sql;

        public EntityRepository(ISql sql)
        {
            _sql = sql;
        }

        public async Task<Guid> AddAsync(EntityInfo objEntityInfo)
        {
            return await _sql.InsertAsync<EntityInfo>(objEntityInfo);
        }

        public async Task<string> GetTableNameAsync(Guid id)
        {
            return await _sql.GetColumnValueAsync<EntityInfo, string>(id, "TableName");
        }

        public async Task<bool> GetIsReadonlyAsync(Guid id)
        {
            return await _sql.GetColumnValueAsync<EntityInfo, bool>(id, "IsReadonly");
        }

        public async Task<EntityInfo> GetAsync(Guid id)
        {
            return await _sql.GetAsync<EntityInfo>(id);
        }

        public async Task<IReadOnlyList<EntityInfo>> GetsAsync(Guid scenarioId, params string[] columns)
        {
            return await _sql.GetByScopeAsync<EntityInfo>(scenarioId, columns);
        }

        public async Task UpdateGroupAsync(Guid? groupId, Guid entityId)
        {
            await _sql.UpdateColumnAsync<EntityInfo>("GroupId", groupId, entityId);
        }

        public async Task<bool> UpdateAsync(EntityInfo objEntityInfo, params string[] columns)
        {
            return await _sql.UpdateAsync<EntityInfo>(objEntityInfo, columns);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _sql.DeleteAsync<EntityInfo>(id);
        }

        #region Store Procedures

        public async Task<(
            IReadOnlyList<EntityInfo> Entities,
            IReadOnlyList<EntityColumnInfo> Columns,
            int? TotalCount)>
            GetsAsync(
            Guid scenarioId,
            int pageIndex,
            int pageSize,
            string searchText,
            int? entityType,
            bool? isReadonly,
            string sortBy)
        {
            var results = await _sql.ExecuteStoredProcedureMultipleAsync<int?, EntityInfo, EntityColumnInfo>(
                "dbo.BusinessEngine_Studio_GetEntitiesWithColumns", "BE_Entities_Studio_GetEntitiesWithColumns",
                    new
                    {
                        ScenarioId = scenarioId,
                        SearchText = searchText,
                        EntityType = entityType,
                        IsReadonly = isReadonly,
                        PageIndex = pageIndex,
                        PageSize = pageSize,
                        SortBy = sortBy
                    });

            var totalCount = results.Item1.Any()
                ? results.Item1.First()
                : 0;

            return (results.Item2, results.Item3, totalCount);
        }

        public async Task<(IReadOnlyList<EntityInfo> Entities, IReadOnlyList<EntityColumnInfo> Columns)> GetsAsync(Guid scenarioId, string sortBy)
        {
            return await _sql.ExecuteStoredProcedureMultipleAsync<EntityInfo, EntityColumnInfo>(
                "dbo.BusinessEngine_Studio_GetEntitiesWithColumnsListItem", "BE_Entities_Studio_GetEntitiesWithColumnsListItem",
                    new
                    {
                        ScenarioId = scenarioId,
                        SortBy = sortBy
                    });
        }

        #endregion
    }
}
