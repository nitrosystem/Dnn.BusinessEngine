using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Dashboard;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Dashboard
{
    public interface IDashboardPageRepository
    {
        Task<Guid> AddAsync(DashboardPageInfo objDashboardPageInfo);

        Task<DashboardPageInfo> GetAsync(Guid id);
        Task<IReadOnlyList<DashboardPageInfo>> GetsAsync(Guid dashboardId, params string[] sortBy);

        Task<bool> UpdateAsync(DashboardPageInfo objDashboardPageInfo, params string[] columns);

        Task<bool> DeleteAsync(Guid id);
    }
}
