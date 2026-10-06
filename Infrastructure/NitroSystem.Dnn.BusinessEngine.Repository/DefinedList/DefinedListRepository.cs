using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.DefinedList;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.DefinedList;

namespace NitroSystem.Dnn.BusinessEngine.Repository.DefinedList
{
    public class DefinedListRepository : IDefinedListRepository
    {
        private readonly ISql _sql;

        public DefinedListRepository(ISql sql)
        {
            _sql = sql;
        }

        public async Task<Guid> AddAsync(DefinedListInfo objDefinedListInfo)
        {
            return await _sql.InsertAsync<DefinedListInfo>(objDefinedListInfo);
        }


        public async Task<DefinedListInfo> GetAsync(string listName)
        {
            return await _sql.GetByColumnAsync<DefinedListInfo>("ListName", listName);
        }
        public async Task<DefinedListInfo> GetAsync(Guid id)
        {
            return await _sql.GetAsync<DefinedListInfo>(id);
        }

        public async Task<IReadOnlyList<DefinedListInfo>> GetsAsync(Guid scenarioId, params string[] columns)
        {
            return await _sql.GetByScopeAsync<DefinedListInfo>(scenarioId, columns);
        }

        public async Task<bool> UpdateAsync(DefinedListInfo objDefinedListInfo, params string[] columns)
        {
            return await _sql.UpdateAsync<DefinedListInfo>(objDefinedListInfo, columns);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _sql.DeleteAsync<DefinedListInfo>(id);
        }
    }
}
