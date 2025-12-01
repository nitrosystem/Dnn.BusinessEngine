using System.Linq;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Views;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Dashboard;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataService.Module
{
    public static class DashboardMappingProfile
    {
        public static void Register()
        {
            #region Dashboard 

            HybridMapper.BeforeMap<DashboardView, DashboardViewModel>(
                (src, dest) => dest.AuthorizationViewDashboard = src.AuthorizationViewDashboard?.Split(','));

            HybridMapper.BeforeMap<DashboardViewModel, DashboardInfo>(
                (src, dest) => dest.AuthorizationViewDashboard = string.Join(",", src.AuthorizationViewDashboard ?? Enumerable.Empty<string>()));

            HybridMapper.BeforeMap<DashboardInfo, DashboardViewModel>(
                (src, dest) => dest.AuthorizationViewDashboard = src.AuthorizationViewDashboard?.Split(','));

            #endregion

            #region Dashboard Module 

            HybridMapper.BeforeMap<DashboardViewModel, ModuleInfo>(
                (src, dest) => dest.Wrapper = (int)ModuleWrapper.DnnPage);

            HybridMapper.BeforeMap<DashboardViewModel, ModuleInfo>(
                (src, dest) => dest.ModuleType = (int)ModuleType.Dashboard);

            #endregion

            #region Dashboard Page

            HybridMapper.BeforeMap<DashboardPageInfo, DashboardPageViewModel>(
                (src, dest) => dest.PageType = (DashboardPageType)src.PageType);

            HybridMapper.BeforeMap<DashboardPageInfo, DashboardPageViewModel>(
                (src, dest) => dest.AuthorizationViewPage = src.AuthorizationViewPage?.Split(','));

            HybridMapper.BeforeMap<DashboardPageViewModel, DashboardPageInfo>(
                (src, dest) => dest.PageType = (int)src.PageType);

            HybridMapper.BeforeMap<DashboardPageViewModel, DashboardPageInfo>(
                (src, dest) => dest.AuthorizationViewPage = string.Join(",", src.AuthorizationViewPage ?? Enumerable.Empty<string>()));

            #endregion

            #region Dashboard Page Module

            HybridMapper.BeforeMap<DashboardPageModuleView, DashboardPageModuleViewModel>(
                (src, dest) => dest.ModuleType = (ModuleType)src.ModuleType);

            HybridMapper.BeforeMap<DashboardPageModuleView, DashboardPageModuleViewModel>(
                (src, dest) => dest.Wrapper = (ModuleWrapper)src.Wrapper);

            HybridMapper.BeforeMap<DashboardPageModuleViewModel, ModuleInfo>(
                (src, dest) => dest.Wrapper = (int)ModuleWrapper.Dashboard);

            HybridMapper.BeforeMap<DashboardPageModuleViewModel, ModuleInfo>(
                (src, dest) => dest.ModuleType = (int)src.ModuleType);

            HybridMapper.BeforeMap<DashboardPageModuleViewModel, ModuleInfo>(
                (src, dest) => dest.Id = src.ModuleId);


            #endregion
        }
    }
}
