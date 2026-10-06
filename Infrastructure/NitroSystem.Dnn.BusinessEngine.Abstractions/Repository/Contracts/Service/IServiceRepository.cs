using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Service;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Views.Service;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Service
{
    public interface IServiceRepository
    {
        Task<Guid> AddAsync(ServiceInfo objServiceInfo);

        Task<string> GetServiceTypeAsync(Guid id);
        Task<string> GetBusinessControllerClassAsync(string serviceType);
        Task<ServiceInfo> GetAsync(Guid id);
        Task<IReadOnlyList<ServiceInfo>> GetsAsync(Guid scenarioId, params string[] columns);
        Task<ServiceView> GetViewAsync(Guid id);
        Task<IReadOnlyList<ServiceView>> GetsViewAsync(Guid scenarioId, params string[] columns);
        Task<(IReadOnlyList<ServiceView> Services, IReadOnlyList<ServiceParamInfo> Params, int? TotalCount)> GetsViewAsync(
                    Guid scenarioId,
                    int pageIndex,
                    int pageSize,
                    string searchText,
                    string serviceDomain,
                    string serviceType,
                    string sortBy);

        Task UpdateGroupAsync(Guid? groupId, Guid entityId);
        Task<bool> UpdateAsync(ServiceInfo objServiceInfo, params string[] columns);

        Task<bool> DeleteAsync(Guid id);
    }
}
