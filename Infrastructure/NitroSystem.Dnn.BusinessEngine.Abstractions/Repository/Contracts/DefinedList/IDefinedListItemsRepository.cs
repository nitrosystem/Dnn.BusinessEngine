using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.DefinedList;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.DefinedList
{
    public interface IDefinedListItemRepository
    {
        Task<Guid> AddAsync(DefinedListItemInfo objDefinedListItemInfo);
        Task BulkInsertAsync(IEnumerable<DefinedListItemInfo> items);

        Task<DefinedListItemInfo> GetAsync(Guid id);
        Task<IReadOnlyList<DefinedListItemInfo>> GetsByAncestorAsync(Guid scenarioId);
        Task<IReadOnlyList<DefinedListItemInfo>> GetsAsync(Guid listId, params string[] columns);

        Task<bool> UpdateAsync(DefinedListItemInfo objDefinedListItemInfo, params string[] columns);

        Task<bool> DeleteAsync(Guid id);
        Task<bool> DeletesAsync(Guid listId);
    }
}
