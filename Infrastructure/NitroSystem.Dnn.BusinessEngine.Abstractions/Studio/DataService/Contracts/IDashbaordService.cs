using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Dashboard;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts
{
    public interface IDashboardService
    {
        Task<Guid?> GetDashboardIdAsync(Guid moduleId);
        Task<DashboardViewModel> GetDashboardViewModelAsync(Guid moduleId);
        Task<(Guid, Guid)> SaveDashboardAsync(DashboardViewModel dashboard);

        Task<IEnumerable<DashboardPageViewModel>> GetDashboardPagesViewModelAsync(Guid dashboardModuleId);
        Task<IEnumerable<DashboardPageListItem>> GetDashboardPagesListItemAsync(Guid dashboardModuleId);
        Task<DashboardPageViewModel> GetDashboardPageViewModelAsync(Guid pageId);
        Task<(Guid?, Guid?, Guid?)> SaveDashboardPageAsync(DashboardPageViewModel page);
        Task SortDashboardPages(DashboardPagesOrder dashboard);
        Task UpdatePageParent(UpdateDashboardPageParent page);
        Task<bool> DeletePageAsync(Guid id);
    }
}
