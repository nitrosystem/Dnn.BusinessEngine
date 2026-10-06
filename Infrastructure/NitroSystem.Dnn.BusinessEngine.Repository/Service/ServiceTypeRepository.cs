using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Service;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Service;

namespace NitroSystem.Dnn.BusinessEngine.Repository.Service
{
    public class ServiceTypeRepository: IServiceTypeRepository
    {
        private readonly ISql _sql;

        public ServiceTypeRepository(ISql sql)
        {
            _sql = sql;
        }

        public async Task<Guid> AddAsync(ServiceTypeInfo objServiceTypeInfo)
        {
            return await _sql.InsertAsync<ServiceTypeInfo>(objServiceTypeInfo);
        }

        public async Task<ServiceTypeInfo> GetAsync(Guid id)
        {
            return await _sql.GetAsync<ServiceTypeInfo>(id);
        }

        public async Task<IReadOnlyList<ServiceTypeInfo>> GetsAsync(params string[] columns)
        {
            return await _sql.GetAllAsync<ServiceTypeInfo>(columns);
        }

        public async Task<bool> UpdateAsync(ServiceTypeInfo objServiceTypeInfo, params string[] columns)
        {
            return await _sql.UpdateAsync<ServiceTypeInfo>(objServiceTypeInfo, columns);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _sql.DeleteAsync<ServiceTypeInfo>(id);
        }
    }
}
