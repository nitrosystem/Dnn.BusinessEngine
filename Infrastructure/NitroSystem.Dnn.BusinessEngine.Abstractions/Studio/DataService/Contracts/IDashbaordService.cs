using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ModuleBuilder.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Dashboard;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Template;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Dashboard;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts
{
    public interface IDashboardService
    {
        Task<Guid?> GetDashboardIdAsync(Guid moduleId);
        Task<DashboardViewModel> GetDashboardViewModelAsync(Guid moduleId);
        Task<(Guid, Guid)> SaveDashboardAsync(DashboardViewModel dashboard);

        Task<DashboardAppearanceViewModel> GetDashboardAppearanceAsync(Guid moduleId);
        Task SaveDashboardAppearanceAsync(DashboardAppearanceViewModel dashboard);
        Task<IEnumerable<DashboardSkinViewModel>> GetDashboardSkinsViewModelAsync();

        Task<IEnumerable<DashboardPageViewModel>> GetDashboardPagesViewModelAsync(Guid moduleId);
        Task<DashboardPageViewModel> GetDashboardPageViewModelAsync(Guid pageId);
        Task<IEnumerable<DashboardPageListItem>> GetDashboardPagesListItemAsync(Guid dashboardModuleId);
        Task<(Guid?, Guid?, Guid?)> SaveDashboardPageAsync(Guid dashboardModuleId, DashboardPageViewModel page);
        Task SortDashboardPages(DashboardPagesOrder dashboard);
        Task UpdatePageParent(UpdateDashboardPageParent page);
        Task<bool> DeletePageAsync(Guid id);
        Task<IEnumerable<TemplateViewModel>> GetTemplates(ModuleType moduleType, Guid moduleId);
    }
}
