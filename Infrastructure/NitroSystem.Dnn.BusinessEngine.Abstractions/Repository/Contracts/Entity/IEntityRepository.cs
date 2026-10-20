using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Procedures.Entity;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Entity;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Entity
{
    public interface IEntityRepository
    {
        Task<Guid> AddAsync(EntityInfo objEntityInfo);
        Task BulkInsertAsync(IReadOnlyList<EntityInfo> entities);

        Task<string> GetTableNameAsync(Guid id);
        Task<bool> GetIsReadonlyAsync(Guid id);
        Task<EntityInfo> GetAsync(Guid id);
        Task<IReadOnlyList<EntityInfo>> GetsAsync(Guid scenarioId, params string[] columns);
        Task<(IReadOnlyList<EntityInfo> Entities, IReadOnlyList<EntityColumnInfo> Columns)> GetsAsync(Guid scenarioId, string sortBy);
        Task<(IReadOnlyList<EntityInfo> Entities, IReadOnlyList<EntityColumnInfo> Columns, int? TotalCount)> GetsAsync(
            Guid scenarioId,
            int pageIndex,
            int pageSize,
            string searchText,
            int? entityType,
            bool? isReadonly,
            string sortBy);
        Task<IReadOnlyList<EntityRelationshipSpResult>> GetRelationshipsAsync(Guid entityId);

        Task UpdateGroupAsync(Guid? groupId, Guid entityId);

        Task<bool> UpdateAsync(EntityInfo objEntityInfo, params string[] columns);

        Task<bool> DeleteAsync(Guid id);
    }
}
