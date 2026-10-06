using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Dashboard;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts
{
    public interface IDashboardService
    {
        #region Dashboard
        Task<Guid> GetDashboardIdAsync(Guid moduleId);
        Task<DashboardViewModel> GetDashboardViewModelAsync(Guid moduleId);

        Task<(Guid DashboardId, Guid ModuleId)> CreateDashboardAsync(DashboardViewModel dashboard);

        #endregion

        #region Dashboard Page & Module

        Task<IEnumerable<DashboardPageViewModel>> GetDashboardPagesViewModelAsync(Guid dashboardModuleId);
        Task<IEnumerable<DashboardPageListItem>> GetDashboardPagesListItemAsync(Guid dashboardModuleId);
        Task<DashboardPageViewModel> GetDashboardPageViewModelAsync(Guid pageId);
        Task<(Guid?, Guid?, Guid?)> CreateDashboardPageAsync(DashboardPageViewModel page, int userId, string basePath);
        Task UpdatePageParentAsync(UpdateDashboardPageParent page);
        Task SortDashboardPagesAsync(ItemsOrder dashboard);
        Task<bool> DeletePageAsync(Guid id);

        #endregion
    }
}
