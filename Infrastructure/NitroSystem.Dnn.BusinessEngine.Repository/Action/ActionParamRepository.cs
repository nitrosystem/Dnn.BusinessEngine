using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Action;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Action;

namespace NitroSystem.Dnn.BusinessEngine.Repository.Action
{
    public class ActionParamRepository: IActionParamRepository
    {
        private readonly ISql _sql;

        public ActionParamRepository(ISql sql)
        {
            _sql = sql;
        }

        public async Task<Guid> AddAsync(ActionParamInfo objActionParamInfo)
        {
            return await _sql.InsertAsync<ActionParamInfo>(objActionParamInfo);
        }

        public async Task BulkInsertAsync(IEnumerable<ActionParamInfo> actionParams)
        {
            await _sql.BulkInsertAsync<ActionParamInfo>(actionParams);
        }

        public async Task<ActionParamInfo> GetAsync(Guid id)
        {
            return await _sql.GetAsync<ActionParamInfo>(id);
        }

        public async Task<IReadOnlyList<ActionParamInfo>> GetsByAncestorAsync(Guid moduleId, params string[] columns)
        {
            return await _sql.GetChildsByParentColumnAsync<ActionInfo, ActionParamInfo>
             (
                 "ModuleId", "ActionId", moduleId, columns
             );
        }

        public async Task<IReadOnlyList<ActionParamInfo>> GetsAsync(Guid actionId, params string[] columns)
        {
            return await _sql.GetByScopeAsync<ActionParamInfo>(actionId, columns);
        }

        public async Task<IReadOnlyList<ActionParamInfo>> GetsAsync(params string[] columns)
        {
            return await _sql.GetAllAsync<ActionParamInfo>(columns);
        }

        public async Task<bool> UpdateAsync(ActionParamInfo objActionParamInfo, params string[] columns)
        {
            return await _sql.UpdateAsync<ActionParamInfo>(objActionParamInfo, columns);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _sql.DeleteAsync<ActionParamInfo>(id);
        }

        public async Task<bool> DeletesAsync(Guid actionId)
        {
            return await _sql.DeleteByScopeAsync<ActionParamInfo>(actionId);
        }
    }
}
