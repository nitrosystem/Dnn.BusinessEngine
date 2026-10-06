using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Service;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Service;

namespace NitroSystem.Dnn.BusinessEngine.Repository.Service
{
    public class ServiceParamRepository: IServiceParamRepository
    {
        private readonly ISql _sql;

        public ServiceParamRepository(ISql sql)
        {
            _sql = sql;
        }

        public async Task<Guid> AddAsync(ServiceParamInfo objServiceParamInfo)
        {
            return await _sql.InsertAsync<ServiceParamInfo>(objServiceParamInfo);
        }

        public async Task BulkInsertAsync(IEnumerable<ServiceParamInfo> serviceParams)
        {
            await _sql.BulkInsertAsync<ServiceParamInfo>(serviceParams);
        }

        public async Task<ServiceParamInfo> GetAsync(Guid id)
        {
            return await _sql.GetAsync<ServiceParamInfo>(id);
        }

        public async Task<IReadOnlyList<ServiceParamInfo>> GetsAsync(Guid serviceId, params string[] columns)
        {
            return await _sql.GetByScopeAsync<ServiceParamInfo>(serviceId, columns);
        }

        public async Task<IReadOnlyList<ServiceParamInfo>> GetsAsync(params string[] columns)
        {
            return await _sql.GetAllAsync<ServiceParamInfo>(columns);
        }

        public async Task<bool> UpdateAsync(ServiceParamInfo objServiceParamInfo, params string[] columns)
        {
            return await _sql.UpdateAsync<ServiceParamInfo>(objServiceParamInfo, columns);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _sql.DeleteAsync<ServiceParamInfo>(id);
        }

        public async Task<bool> DeletesAsync(Guid serviceId)
        {
            return await _sql.DeleteByScopeAsync<ServiceParamInfo>(serviceId);
        }
    }
}
