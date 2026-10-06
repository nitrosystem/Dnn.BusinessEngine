using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Action;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Action
{
    public interface IActionTypeRepository
    {
        Task<Guid> AddAsync(ActionTypeInfo objActionTypeInfo);

        Task<ActionTypeInfo> GetAsync(Guid id);
        Task<IReadOnlyList<ActionTypeInfo>> GetsAsync(params string[] columns);

        Task<bool> UpdateAsync(ActionTypeInfo objActionTypeInfo, params string[] columns);

        Task<bool> DeleteAsync(Guid id);
    }
}
