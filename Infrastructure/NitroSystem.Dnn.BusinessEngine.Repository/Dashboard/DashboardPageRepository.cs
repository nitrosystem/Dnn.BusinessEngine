using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Dashboard;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Dashboard;

namespace NitroSystem.Dnn.BusinessEngine.Repository.Dashboard
{
    public class DashboardPageRepository : IDashboardPageRepository
    {
        private readonly ISql _sql;

        public DashboardPageRepository(ISql sql)
        {
            _sql = sql;
        }

        public async Task<Guid> AddAsync(DashboardPageInfo objDashboardPageInfo)
        {
            return await _sql.InsertAsync<DashboardPageInfo>(objDashboardPageInfo);
        }

        public async Task<DashboardPageInfo> GetAsync(Guid id)
        {
            return await _sql.GetAsync<DashboardPageInfo>(id);
        }

        public async Task<IReadOnlyList<DashboardPageInfo>> GetsAsync(Guid dashboardId, params string[] columns)
        {
            return await _sql.GetByScopeAsync<DashboardPageInfo>(dashboardId, columns);
        }

        public async Task<bool> UpdateAsync(DashboardPageInfo objDashboardPageInfo, params string[] columns)
        {
            return await _sql.UpdateAsync<DashboardPageInfo>(objDashboardPageInfo, columns);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _sql.DeleteAsync<DashboardPageInfo>(id);
        }
    }
}
