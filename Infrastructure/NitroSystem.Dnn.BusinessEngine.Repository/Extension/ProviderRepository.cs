using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Extension;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Extension;

namespace NitroSystem.Dnn.BusinessEngine.Repository.Extension
{
    public class ProviderRepository: IProviderRepository
    {
        private readonly ISql _sql;

        public ProviderRepository(ISql sql)
        {
            _sql = sql;
        }

        public async Task<Guid> AddAsync(ProviderInfo objProviderInfo)
        {
            return await _sql.InsertAsync<ProviderInfo>(objProviderInfo);
        }

        public async Task<ProviderInfo> GetAsync(Guid id)
        {
            return await _sql.GetAsync<ProviderInfo>(id);
        }

        public async Task<IReadOnlyList<ProviderInfo>> GetsAsync(params string[] columns)
        {
            return await _sql.GetAllAsync<ProviderInfo>(columns);
        }

        public async Task<bool> UpdateAsync(ProviderInfo objProviderInfo, params string[] columns)
        {
            return await _sql.UpdateAsync<ProviderInfo>(objProviderInfo, columns);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _sql.DeleteAsync<ProviderInfo>(id);
        }
    }
}
