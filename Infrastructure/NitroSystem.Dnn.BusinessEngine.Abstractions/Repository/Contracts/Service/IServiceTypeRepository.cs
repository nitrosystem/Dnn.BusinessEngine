using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Service;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Service
{
    public interface IServiceTypeRepository
    {
        Task<Guid> AddAsync(ServiceTypeInfo objServiceTypeInfo);

        Task<ServiceTypeInfo> GetAsync(Guid id);
        Task<IReadOnlyList<ServiceTypeInfo>> GetsAsync(params string[] columns);

        Task<bool> UpdateAsync(ServiceTypeInfo objServiceTypeInfo, params string[] columns);

        Task<bool> DeleteAsync(Guid id);
    }
}
