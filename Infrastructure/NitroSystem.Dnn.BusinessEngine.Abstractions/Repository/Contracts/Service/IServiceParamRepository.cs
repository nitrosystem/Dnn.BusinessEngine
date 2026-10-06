using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Service;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Service
{
    public interface IServiceParamRepository
    {
        Task<Guid> AddAsync(ServiceParamInfo objServiceParamInfo);
        Task BulkInsertAsync(IEnumerable<ServiceParamInfo> serviceParams);

        Task<ServiceParamInfo> GetAsync(Guid id);
        Task<IReadOnlyList<ServiceParamInfo>> GetsAsync(Guid serviceId, params string[] columns);
        Task<IReadOnlyList<ServiceParamInfo>> GetsAsync(params string[] columns);
        
        Task<bool> UpdateAsync(ServiceParamInfo objServiceParamInfo, params string[] columns);

        Task<bool> DeleteAsync(Guid id);
        Task<bool> DeletesAsync(Guid serviceId);
    }
}
