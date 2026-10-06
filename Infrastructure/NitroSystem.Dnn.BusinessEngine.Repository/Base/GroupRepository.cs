using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Base;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Base;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Views.Base;

namespace NitroSystem.Dnn.BusinessEngine.Repository.Base
{
    public class GroupRepository : IGroupRepository
    {
        private readonly ISql _sql;

        public GroupRepository(ISql sql)
        {
            _sql = sql;
        }

        public async Task<Guid> AddAsync(GroupInfo objGroupInfo)
        {
            return await _sql.InsertAsync<GroupInfo>(objGroupInfo);
        }

        public async Task<GroupInfo> GetAsync(Guid id)
        {
            return await _sql.GetAsync<GroupInfo>(id);
        }

        public async Task<IReadOnlyList<GroupInfo>> GetsAsync(Guid scenarioId, params string[] columns)
        {
            return await _sql.GetByScopeAsync<GroupInfo>(scenarioId, columns);
        }

        public async Task<IReadOnlyList<GroupInfo>> GetsByDomainAsync(Guid scenarioId, string groupDomain)
        {
            return await _sql.GetItemsByColumnsAsync<GroupInfo>(
                new string[2] { "ScenarioId", "GroupDomain" },
                new
                {
                    ScenarioId = scenarioId,
                    GroupDomain = groupDomain
                }
            );
        }

        public async Task<IReadOnlyList<ExplorerItemView>> GetExplorerItemsViewModelAsync(Guid scenarioId, string columns)
        {
            return await _sql.GetByScopeAsync<ExplorerItemView>(scenarioId, columns);
        }

        public async Task<bool> UpdateAsync(GroupInfo objGroupInfo, params string[] columns)
        {
            return await _sql.UpdateAsync<GroupInfo>(objGroupInfo, columns);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _sql.DeleteAsync<GroupInfo>(id);
        }
    }
}
