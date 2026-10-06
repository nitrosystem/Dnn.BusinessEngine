using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Service;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Service;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Views.Service;

namespace NitroSystem.Dnn.BusinessEngine.Repository.Service
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly ISql _sql;

        public ServiceRepository(ISql sql)
        {
            _sql = sql;
        }

        public async Task<Guid> AddAsync(ServiceInfo objServiceInfo)
        {
            return await _sql.InsertAsync<ServiceInfo>(objServiceInfo);
        }

        public async Task<string> GetServiceTypeAsync(Guid id)
        {
            return await _sql.GetColumnValueAsync<ServiceInfo, string>(id, "ServiceType");
        }

        public async Task<string> GetBusinessControllerClassAsync(string serviceType)
        {
            return await _sql.GetColumnValueAsync<ServiceTypeInfo, string>("BusinessControllerClass", "ServiceType", serviceType);
        }

        public async Task<ServiceInfo> GetAsync(Guid id)
        {
            return await _sql.GetAsync<ServiceInfo>(id);
        }

        public async Task<IReadOnlyList<ServiceInfo>> GetsAsync(Guid scenarioId, params string[] columns)
        {
            return await _sql.GetByScopeAsync<ServiceInfo>(scenarioId, columns);
        }

        public async Task<ServiceView> GetViewAsync(Guid id)
        {
            return await _sql.GetAsync<ServiceView>(id);
        }

        public async Task<IReadOnlyList<ServiceView>> GetsViewAsync(Guid scenarioId, params string[] columns)
        {
            return await _sql.GetByScopeAsync<ServiceView>(scenarioId, columns);
        }

        public async Task UpdateGroupAsync(Guid? groupId, Guid entityId)
        {
            await _sql.UpdateColumnAsync<ServiceInfo>("GroupId", groupId, entityId);
        }

        public async Task<bool> UpdateAsync(ServiceInfo objServiceInfo, params string[] columns)
        {
            return await _sql.UpdateAsync<ServiceInfo>(objServiceInfo, columns);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _sql.DeleteAsync<ServiceInfo>(id);
        }

        #region Stored Procedures

        public async Task<(IReadOnlyList<ServiceView> Services, IReadOnlyList<ServiceParamInfo> Params, int? TotalCount)> GetsViewAsync(
            Guid scenarioId,
            int pageIndex,
            int pageSize,
            string searchText,
            string serviceDomain,
            string serviceType,
            string sortBy)
        {
            var results = await _sql.ExecuteStoredProcedureMultipleAsync<int?, ServiceView, ServiceParamInfo>(
                    "dbo.BusinessEngine_Studio_GetServicesWithParams", "BE_Services_Studio_GetServicesWithParams_",
                    new
                    {
                        ScenarioId = scenarioId,
                        SearchText = searchText,
                        ServiceDomain = serviceDomain,
                        ServiceType = serviceType,
                        PageIndex = pageIndex,
                        PageSize = pageSize,
                        SortBy = sortBy
                    });

            var totalCount = results.Item1.Any()
                ? results.Item1.First()
                : 0;

            return (results.Item2, results.Item3, totalCount);
        }

        #endregion
    }

}
