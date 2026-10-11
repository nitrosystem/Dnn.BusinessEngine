using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Dashboard;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Views.Dashboard;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Dashboard
{
    public interface IDashboardRepository
    {
        Task<Guid> AddAsync(DashboardInfo objDashboardInfo);
        Task BulkInsertAsync(IReadOnlyList<DashboardInfo> pages);

        Task<Guid> GetIdByModuleIdAsync(Guid moduleId);
        Task<DashboardView> GetViewByModuleIdAsync(Guid moduleId);
        string GetAuthorizationViewDashboard(Guid moduleId);
        Task<DashboardInfo> GetByModuleIdAsync(Guid moduleId);
        Task<DashboardInfo> GetAsync(Guid id);
        Task<IReadOnlyList<DashboardInfo>> GetsAsync(params string[] columns);
        Task<IReadOnlyList<DashboardInfo>> GetsByAncestorAsync(Guid scenarioId);

        Task<bool> UpdateAuthorizationViewDashboardAsync(DashboardInfo objDashboardInfo);
        Task<bool> UpdateAsync(DashboardInfo objDashboardInfo, params string[] columns);

        Task<bool> DeleteAsync(Guid id);
    }
}
