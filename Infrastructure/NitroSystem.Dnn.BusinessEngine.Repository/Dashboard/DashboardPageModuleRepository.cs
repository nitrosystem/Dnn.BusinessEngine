using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Dashboard;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Dashboard;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Views.Dashboard;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Procedures.Dashboard;

namespace NitroSystem.Dnn.BusinessEngine.Repository.Dashboard
{
    public class DashboardPageModuleRepository: IDashboardPageModuleRepository
    {
        private readonly ISql _sql;

        public DashboardPageModuleRepository(ISql sql)
        {
            _sql = sql;
        }

        public async Task<Guid> AddAsync(DashboardPageModuleInfo objDashboardPageModuleInfo)
        {
            return await _sql.InsertAsync<DashboardPageModuleInfo>(objDashboardPageModuleInfo);
        }

        public async Task<DashboardPageModuleInfo> GetAsync(Guid id)
        {
            return await _sql.GetAsync<DashboardPageModuleInfo>(id);
        }

        public async Task<IReadOnlyList<DashboardPageModuleView>> GetsViewAsync(Guid id)
        {
            return await _sql.GetByScopeAsync<DashboardPageModuleView>(id);
        }

        public async Task<bool> UpdateAsync(DashboardPageModuleInfo objDashboardPageModuleInfo, params string[] columns)
        {
            return await _sql.UpdateAsync<DashboardPageModuleInfo>(objDashboardPageModuleInfo, columns);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _sql.DeleteAsync<DashboardPageModuleInfo>(id);
        }

        #region Stored Procedures

        public DashboardPageModuleSpResult Get(Guid dashboardModuleId, string pageName)
        {
            return _sql.ExecuteStoredProcedure<DashboardPageModuleSpResult>(
                "dbo.BusinessEngine_App_GetDashboardPageModule", "BE_Dashboards_Pages_Modules_App_",
                new
                {
                    DashboardModuleId = dashboardModuleId,
                    PageName = pageName
                });
        }

        #endregion
    }
}
