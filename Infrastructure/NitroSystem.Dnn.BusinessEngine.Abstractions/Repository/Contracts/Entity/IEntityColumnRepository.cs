using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Entity;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Entity
{
    public interface IEntityColumnRepository
    {
        Task<Guid> AddAsync(EntityColumnInfo objEntityColumnInfo);
        Task BulkInsertAsync(IReadOnlyList<EntityColumnInfo> columns);

        Task<EntityColumnInfo> GetAsync(Guid id);
        Task<IReadOnlyList<EntityColumnInfo>> GetsAsync(Guid entityId, params string[] columns);
        Task<IReadOnlyList<EntityColumnInfo>> GetsAsync(Guid entityId);

        Task<bool> UpdateAsync(EntityColumnInfo objEntityColumnInfo, params string[] columns);

        Task<bool> DeleteAsync(Guid id);
        Task<bool> DeletesAsync(Guid entityId);
    }
}
