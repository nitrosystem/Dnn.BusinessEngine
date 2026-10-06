using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Base;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Views.Base;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Base
{
    public interface IGroupRepository
    {
        Task<Guid> AddAsync(GroupInfo objGroupInfo);

        Task<GroupInfo> GetAsync(Guid id);
        Task<IReadOnlyList<GroupInfo>> GetsAsync(Guid scenarioId, params string[] columns);
        Task<IReadOnlyList<GroupInfo>> GetsByDomainAsync(Guid scenarioId, string groupDomain);
        Task<IReadOnlyList<ExplorerItemView>> GetExplorerItemsViewModelAsync(Guid scenarioId, string sortBy);

        Task<bool> UpdateAsync(GroupInfo objGroupInfo, params string[] columns);

        Task<bool> DeleteAsync(Guid id);
    }
}
