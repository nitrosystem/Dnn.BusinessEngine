using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Views;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ModuleBuilder.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ModuleBuilder.Models;
using NitroSystem.Dnn.BusinessEngine.Shared.Extensions;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Dto;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Procedures;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ListItems;
using System.Web.UI;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Dashboard;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Template;

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

            //#region Dashboard Appearance 

            //HybridMapper.BeforeMap<DashboardAppearanceView, DashboardAppearanceViewModel>(
            //    (src, dest) => dest.SkinImage = src.SkinImage?.ReplaceFrequentTokens());

            //HybridMapper.BeforeMap<DashboardAppearanceView, DashboardAppearanceViewModel>(
            //    (src, dest) => dest.TemplateImage = src.TemplateImage?.ReplaceFrequentTokens());

            //HybridMapper.BeforeMap<DashboardAppearanceView, DashboardAppearanceViewModel>(
            //    (src, dest) => dest.TemplatePath = src.TemplatePath?.ReplaceFrequentTokens());

            //HybridMapper.BeforeMap<DashboardAppearanceView, DashboardAppearanceViewModel>(
            //    (src, dest) => dest.TemplateCssPath = src.TemplateCssPath?.ReplaceFrequentTokens());

            //HybridMapper.BeforeMap<DashboardSkinInfo, DashboardSkinViewModel>(
            //    (src, dest) => dest.SkinImage = src.SkinImage?.ReplaceFrequentTokens());

            //HybridMapper.BeforeMap<DashboardSkinInfo, DashboardSkinViewModel>(
            //    (src, dest) => dest.CssFiles = src.CssFiles?.Split(','));

            //HybridMapper.BeforeMap<DashboardSkinInfo, DashboardSkinViewModel>(
            //    (src, dest) => dest.JsFiles = src.JsFiles?.Split(','));

            //HybridMapper.BeforeMap<DashboardSkinViewModel, DashboardSkinInfo>(
            //    (src, dest) => dest.CssFiles = string.Join(",", src.CssFiles ?? Enumerable.Empty<string>()));

            //HybridMapper.BeforeMap<DashboardSkinViewModel, DashboardSkinInfo>(
            //    (src, dest) => dest.JsFiles = string.Join(",", src.JsFiles ?? Enumerable.Empty<string>()));

            //HybridMapper.BeforeMap<DashboardSkinTemplateInfo, DashboardSkinTemplateViewModel>(
            //    (src, dest) => dest.TemplateImage = src.TemplateImage?.ReplaceFrequentTokens());

            //HybridMapper.BeforeMap<DashboardSkinTemplateInfo, DashboardSkinTemplateViewModel>(
            //    (src, dest) => dest.TemplatePath = src.TemplatePath?.ReplaceFrequentTokens());

            //HybridMapper.BeforeMap<DashboardSkinTemplateInfo, DashboardSkinTemplateViewModel>(
            //    (src, dest) => dest.ModuleType = (ModuleType)src.ModuleType);

            //HybridMapper.BeforeMap<DashboardSkinTemplateViewModel, DashboardSkinTemplateInfo>(
            //    (src, dest) => dest.ModuleType = (int)src.ModuleType);

            //HybridMapper.BeforeMap<DashboardSkinTemplateInfo, TemplateViewModel>(
            //    (src, dest) => dest.TemplatePath = src.TemplatePath?.ReplaceFrequentTokens());

            //HybridMapper.BeforeMap<DashboardSkinTemplateInfo, TemplateViewModel>(
            //    (src, dest) => dest.TemplateImage = src.TemplateImage?.ReplaceFrequentTokens());

            //#endregion

            #region Dashboard Page

            HybridMapper.BeforeMap<DashboardPageInfo, DashboardPageViewModel>(
                (src, dest) => dest.PageType = (DashboardPageType)src.PageType);

            HybridMapper.BeforeMap<DashboardPageInfo, DashboardPageViewModel>(
                (src, dest) => dest.AuthorizationViewPage = src.AuthorizationViewPage?.Split(','));

            HybridMapper.BeforeMap<DashboardPageInfo, DashboardPageViewModel>(
                (src, dest) => dest.Settings = ReflectionUtil.TryJsonCasting<IDictionary<string, object>>(src.Settings));

            HybridMapper.BeforeMap<DashboardPageView, DashboardPageViewModel>(
                (src, dest) => dest.AuthorizationViewPage = src.AuthorizationViewPage?.Split(','));

            HybridMapper.BeforeMap<DashboardPageView, DashboardPageViewModel>(
                (src, dest) => dest.Settings = ReflectionUtil.TryJsonCasting<IDictionary<string, object>>(src.Settings));

            HybridMapper.BeforeMap<DashboardPageView, DashboardPageViewModel>(
                (src, dest) => dest.PageType = (DashboardPageType)src.PageType);

            HybridMapper.BeforeMap<DashboardPageViewModel, DashboardPageInfo>(
                (src, dest) => dest.PageType = (int)src.PageType);

            HybridMapper.BeforeMap<DashboardPageViewModel, DashboardPageInfo>(
                (src, dest) => dest.AuthorizationViewPage = string.Join(",", src.AuthorizationViewPage ?? Enumerable.Empty<string>()));

            HybridMapper.BeforeMap<DashboardPageViewModel, DashboardPageInfo>(
                (src, dest) => dest.Settings = JsonConvert.SerializeObject(src.Settings));

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

            HybridMapper.BeforeMap<DashboardPageModuleView, ModuleInfo>(
                (src, dest) => dest.Id = src.ModuleId);

            #endregion
        }
    }
}
