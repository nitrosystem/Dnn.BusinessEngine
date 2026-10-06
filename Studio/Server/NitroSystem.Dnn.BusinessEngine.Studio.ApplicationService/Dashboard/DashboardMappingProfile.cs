using System.Linq;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Views.Dashboard;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Dashboard;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Dashboard;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;

namespace NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.Dashboard
{
    public static class DashboardMappingProfile
    {
        public static void Register()
        {
            #region Dashboard 

            HybridMapper.AfterMap<DashboardView, DashboardViewModel>(
                (src, dest) => dest.AuthorizationViewDashboard = src.AuthorizationViewDashboard?.Split(','));

            HybridMapper.AfterMap<DashboardViewModel, DashboardInfo>(
                (src, dest) => dest.AuthorizationViewDashboard = string.Join(",", src.AuthorizationViewDashboard ?? Enumerable.Empty<string>()));

            HybridMapper.AfterMap<DashboardInfo, DashboardViewModel>(
                (src, dest) => dest.AuthorizationViewDashboard = src.AuthorizationViewDashboard?.Split(','));

            #endregion

            #region Dashboard Module 

            HybridMapper.AfterMap<DashboardViewModel, ModuleInfo>(
                (src, dest) => dest.Wrapper = (int)ModuleWrapper.DnnPage);

            HybridMapper.AfterMap<DashboardViewModel, ModuleInfo>(
                (src, dest) => dest.ModuleType = (int)ModuleType.Dashboard);

            #endregion

            #region Dashboard Page

            HybridMapper.AfterMap<DashboardPageInfo, DashboardPageViewModel>(
                (src, dest) => dest.PageType = (DashboardPageType)src.PageType);

            HybridMapper.AfterMap<DashboardPageInfo, DashboardPageViewModel>(
                (src, dest) => dest.AuthorizationViewPage = src.AuthorizationViewPage?.Split(','));

            HybridMapper.AfterMap<DashboardPageViewModel, DashboardPageInfo>(
                (src, dest) => dest.PageType = (int)src.PageType);

            HybridMapper.AfterMap<DashboardPageViewModel, DashboardPageInfo>(
                (src, dest) => dest.AuthorizationViewPage = string.Join(",", src.AuthorizationViewPage ?? Enumerable.Empty<string>()));

            #endregion

            #region Dashboard Page Module

            HybridMapper.AfterMap<DashboardPageModuleView, DashboardPageModuleViewModel>(
                (src, dest) => dest.ModuleType = (ModuleType)src.ModuleType);

            HybridMapper.AfterMap<DashboardPageModuleView, DashboardPageModuleViewModel>(
                (src, dest) => dest.Wrapper = (ModuleWrapper)src.Wrapper);

            HybridMapper.AfterMap<DashboardPageModuleViewModel, ModuleInfo>(
                (src, dest) => dest.Wrapper = (int)ModuleWrapper.Dashboard);

            HybridMapper.AfterMap<DashboardPageModuleViewModel, ModuleInfo>(
                (src, dest) => dest.ModuleType = (int)src.ModuleType);

            HybridMapper.AfterMap<DashboardPageModuleViewModel, ModuleInfo>(
                (src, dest) => dest.Id = src.ModuleId);


            #endregion
        }
    }
}
