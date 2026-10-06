using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Dashboard;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Views.Dashboard;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Procedures.Dashboard;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Dashboard
{
    public interface IDashboardPageModuleRepository
    {
        Task<Guid> AddAsync(DashboardPageModuleInfo objDashboardPageModuleInfo);

        Task<DashboardPageModuleInfo> GetAsync(Guid id);
        Task<IReadOnlyList<DashboardPageModuleView>> GetsViewAsync(Guid id);
        DashboardPageModuleSpResult Get(Guid dashboardModuleId, string pageName);

        Task<bool> UpdateAsync(DashboardPageModuleInfo objDashboardPageModuleInfo, params string[] columns);

        Task<bool> DeleteAsync(Guid id);
    }
}
