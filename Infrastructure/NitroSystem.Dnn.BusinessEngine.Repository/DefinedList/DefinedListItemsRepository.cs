using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.DefinedList;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.DefinedList;

namespace NitroSystem.Dnn.BusinessEngine.Repository.DefinedList
{
    public class DefinedListItemRepository: IDefinedListItemRepository
    {
        private readonly ISql _sql;

        public DefinedListItemRepository(ISql sql)
        {
            _sql = sql;
        }

        public async Task<Guid> AddAsync(DefinedListItemInfo objDefinedListItemInfo)
        {
            return await _sql.InsertAsync<DefinedListItemInfo>(objDefinedListItemInfo);
        }

        public async Task BulkInsertAsync(IEnumerable<DefinedListItemInfo> items)
        {
            await _sql.BulkInsertAsync<DefinedListItemInfo>(items);
        }

        public async Task<DefinedListItemInfo> GetAsync(Guid id)
        {
            return await _sql.GetAsync<DefinedListItemInfo>(id);
        }

        public async Task<IReadOnlyList<DefinedListItemInfo>> GetsByAncestorAsync(Guid scenarioId)
        {
            return await _sql.GetChildsByParentColumnAsync<DefinedListInfo, DefinedListItemInfo>
             (
                 "ScenarioId", "ListId", scenarioId
             );
        }

        public async Task<IReadOnlyList<DefinedListItemInfo>> GetsAsync(Guid listId, params string[] columns)
        {
            return await _sql.GetByScopeAsync<DefinedListItemInfo>(listId, columns);
        }

        public async Task<bool> UpdateAsync(DefinedListItemInfo objDefinedListItemInfo, params string[] columns)
        {
            return await _sql.UpdateAsync<DefinedListItemInfo>(objDefinedListItemInfo, columns);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _sql.DeleteAsync<DefinedListItemInfo>(id);
        }

        public async Task<bool> DeletesAsync(Guid listId)
        {
            return await _sql.DeleteByScopeAsync<DefinedListItemInfo>(listId);
        }
    }
}
