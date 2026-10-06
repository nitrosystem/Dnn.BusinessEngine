using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.DefinedList;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.DefinedList
{
    public interface IDefinedListRepository
    {
        Task<Guid> AddAsync(DefinedListInfo objDefinedListInfo);

        Task<DefinedListInfo> GetAsync(string listName);
        Task<DefinedListInfo> GetAsync(Guid id);
        Task<IReadOnlyList<DefinedListInfo>> GetsAsync(Guid scenarioId, params string[] columns);

        Task<bool> UpdateAsync(DefinedListInfo objDefinedListInfo, params string[] columns);

        Task<bool> DeleteAsync(Guid id);
    }
}
