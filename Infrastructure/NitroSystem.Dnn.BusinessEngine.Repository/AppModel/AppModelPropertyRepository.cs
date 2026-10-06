using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.AppModel;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.AppModel;

namespace NitroSystem.Dnn.BusinessEngine.Repository.AppModel
{
    public class AppModelPropertyRepository : IAppModelPropertyRepository
    {
        private readonly ISql _sql;

        public AppModelPropertyRepository(ISql sql)
        {
            _sql = sql;
        }

        public async Task<Guid> AddAsync(AppModelPropertyInfo objAppModelPropertyInfo)
        {
            return await _sql.InsertAsync<AppModelPropertyInfo>(objAppModelPropertyInfo);
        }

        public async Task BulkInsertAsync(IEnumerable<AppModelPropertyInfo> properties)
        {
            await _sql.BulkInsertAsync<AppModelPropertyInfo>(properties);
        }

        public async Task<AppModelPropertyInfo> GetAsync(Guid id)
        {
            return await _sql.GetAsync<AppModelPropertyInfo>(id);
        }

        public async Task<IReadOnlyList<AppModelPropertyInfo>> GetsAsync(Guid appModelId, params string[] columns)
        {
            return await _sql.GetByScopeAsync<AppModelPropertyInfo>(appModelId, columns);
        }

        public async Task<IReadOnlyList<AppModelPropertyInfo>> GetsAsync(params string[] columns)
        {
            return await _sql.GetAllAsync<AppModelPropertyInfo>(columns);
        }

        public async Task<bool> UpdateAsync(AppModelPropertyInfo objAppModelPropertyInfo, params string[] columns)
        {
            return await _sql.UpdateAsync<AppModelPropertyInfo>(objAppModelPropertyInfo, columns);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _sql.DeleteAsync<AppModelPropertyInfo>(id);
        }

        public async Task<bool> DeletesAsync(Guid appModelId)
        {
            return await _sql.DeleteByScopeAsync<AppModelPropertyInfo>(appModelId);
        }
    }
}
