using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Action;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Action;

namespace NitroSystem.Dnn.BusinessEngine.Repository.Action
{
    public class ActionTypeRepository : IActionTypeRepository
    {
        private readonly ISql _sql;

        public ActionTypeRepository(ISql sql)
        {
            _sql = sql;
        }

        public async Task<Guid> AddAsync(ActionTypeInfo objActionTypeInfo)
        {
            return await _sql.InsertAsync<ActionTypeInfo>(objActionTypeInfo);
        }

        public async Task<ActionTypeInfo> GetAsync(Guid id)
        {
            return await _sql.GetAsync<ActionTypeInfo>(id);
        }

        public async Task<IReadOnlyList<ActionTypeInfo>> GetsAsync(params string[] columns)
        {
            return await _sql.GetAllAsync<ActionTypeInfo>(columns);
        }

        public async Task<bool> UpdateAsync(ActionTypeInfo objActionTypeInfo, params string[] columns)
        {
            return await _sql.UpdateAsync<ActionTypeInfo>(objActionTypeInfo, columns);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _sql.DeleteAsync<ActionTypeInfo>(id);
        }
    }
}
