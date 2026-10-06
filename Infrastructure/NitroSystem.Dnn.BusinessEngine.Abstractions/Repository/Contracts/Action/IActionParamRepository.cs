using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Action;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Action
{
    public interface IActionParamRepository
    {
        Task<Guid> AddAsync(ActionParamInfo objActionParamInfo);
        Task BulkInsertAsync(IEnumerable<ActionParamInfo> actionParams);

        Task<ActionParamInfo> GetAsync(Guid id);
        Task<IReadOnlyList<ActionParamInfo>> GetsByAncestorAsync(Guid moduleId, params string[] columns);
        Task<IReadOnlyList<ActionParamInfo>> GetsAsync(Guid actionId, params string[] columns);
        Task<IReadOnlyList<ActionParamInfo>> GetsAsync(params string[] columns);

        Task<bool> UpdateAsync(ActionParamInfo objActionParamInfo, params string[] columns);

        Task<bool> DeleteAsync(Guid id);
        Task<bool> DeletesAsync(Guid actionId);
    }
}
