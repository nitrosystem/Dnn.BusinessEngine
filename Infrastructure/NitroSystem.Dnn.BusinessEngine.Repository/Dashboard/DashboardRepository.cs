using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Dashboard;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Dashboard;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Views.Dashboard;

namespace NitroSystem.Dnn.BusinessEngine.Repository.Dashboard
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly ISql _sql;

        public DashboardRepository(ISql sql)
        {
            _sql = sql;
        }

        public async Task<Guid> AddAsync(DashboardInfo objDashboardInfo)
        {
            return await _sql.InsertAsync<DashboardInfo>(objDashboardInfo);
        }

        public async Task<Guid> GetIdByModuleIdAsync(Guid moduleId)
        {
            return await _sql.GetColumnValueAsync<DashboardInfo, Guid>("Id", "ModuleId", moduleId);
        }

        public async Task<DashboardView> GetViewByModuleIdAsync(Guid moduleId)
        {
            return await _sql.GetByColumnAsync<DashboardView>("ModuleId", moduleId);
        }

        public string GetAuthorizationViewDashboard(Guid moduleId)
        {
            return _sql.GetColumnValue<DashboardInfo, string>("AuthorizationViewDashboard", "ModuleId", moduleId);
        }

        public async Task<DashboardInfo> GetByModuleIdAsync(Guid moduleId)
        {
            return await _sql.GetByColumnAsync<DashboardInfo>("ModuleId", moduleId);
        }

        public async Task<DashboardInfo> GetAsync(Guid id)
        {
            return await _sql.GetAsync<DashboardInfo>(id);
        }

        public async Task<IReadOnlyList<DashboardInfo>> GetsAsync(params string[] columns)
        {
            return await _sql.GetAllAsync<DashboardInfo>(columns);
        }

        public async Task<bool> UpdateAuthorizationViewDashboardAsync(DashboardInfo objDashboardInfo)
        {
            return await _sql.UpdateAsync(objDashboardInfo, "AuthorizationViewDashboard");
        }

        public async Task<bool> UpdateAsync(DashboardInfo objDashboardInfo, params string[] columns)
        {
            return await _sql.UpdateAsync<DashboardInfo>(objDashboardInfo, columns);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _sql.DeleteAsync<DashboardInfo>(id);
        }
    }
}
