using DotNetNuke.Collections;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Common.TypeCasting;
using NitroSystem.Dnn.BusinessEngine.Core.Mapper;
using NitroSystem.Dnn.BusinessEngine.Core.Models;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Enums;
using NitroSystem.Dnn.BusinessEngine.Common.Reflection;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Dto;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using System.Web.UI;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Dashboard;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Dashboard.Skin;
using DotNetNuke.UI.Skins;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.Enums;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.DashboardAppearance.Models;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Mapping
{
    public static class DashboardMapping
    {
        #region Dashboard Mapping

        public static DashboardDto MapDashboardDto(DashboardView dashboard)
        {
            var mapper = new ExpressionMapper<DashboardView, DashboardDto>();
            mapper.AddCustomMapping(src => src.AuthorizationViewDashboard, dest => dest.AuthorizationViewDashboard,
                source => source.AuthorizationViewDashboard.Split(','),
                condition => !string.IsNullOrEmpty(condition.AuthorizationViewDashboard));
            mapper.AddCustomMapping(src => src, dest => dest.DashboardType,
                source => (DashboardType)source.DashboardType);

            var result = mapper.Map(dashboard);
            return result;
        }

        public static IEnumerable<DashboardViewModel> MapDashboardsViewModel(IEnumerable<DashboardView> dashboards, IEnumerable<DashboardPageInfo> pages, IEnumerable<DashboardPageModuleView> modules)
        {
            return dashboards.Select(dashboard => MapDashboardViewModel(dashboard, pages));
        }

        public static DashboardViewModel MapDashboardViewModel(DashboardView dashboard, IEnumerable<DashboardPageInfo> pages = null)
        {
            var mapper = new ExpressionMapper<DashboardView, DashboardViewModel>();
            mapper.AddCustomMapping(src => src.AuthorizationViewDashboard, dest => dest.AuthorizationViewDashboard,
                source => source.AuthorizationViewDashboard.Split(','),
                condition => !string.IsNullOrEmpty(condition.AuthorizationViewDashboard));
            mapper.AddCustomMapping(src => src, dest => dest.Pages, source => BuildPageTree(pages), condition => pages != null);

            var result = mapper.Map(dashboard);
            return result;
        }

        public static List<DashboardPageDto> BuildPageTree(IEnumerable<DashboardPageInfo> pages)
        {
            var pageLookup = pages.ToLookup(p => p.ParentId);
            return PopulateDashboardPages(null, pageLookup);
        }

        private static List<DashboardPageDto> PopulateDashboardPages(Guid? parentId, ILookup<Guid?, DashboardPageInfo> lookup)
        {
            return lookup[parentId]
                .Select(page => new DashboardPageDto
                {
                    Id = page.Id,
                    DashboardId = page.DashboardId,
                    ParentId = page.ParentId,
                    ExistingPageId = page.ExistingPageId,
                    PageType = (DashboardPageType)page.PageType,
                    PageName = page.PageName,
                    Title = page.Title,
                    Url = page.Url,
                    IsVisible = page.IsVisible,
                    InheritPermissionFromDashboard = page.InheritPermissionFromDashboard,
                    ViewOrder = page.ViewOrder,
                    IsChild = page.ParentId != null,
                    AuthorizationViewPage = (page.AuthorizationViewPage ?? string.Empty).Split(','),
                    Settings = TypeCastingUtil<IDictionary<string, object>>.TryJsonCasting(page.Settings),
                    Pages = PopulateDashboardPages(page.Id, lookup)
                })
                .ToList();
        }

        public static DashboardPageDto MapDashboardPageDto(DashboardPageView page)
        {
            var mapper = new ExpressionMapper<DashboardPageView, DashboardPageDto>();
            mapper.AddCustomMapping(src => src.PageType, dest => dest.PageType, source => (DashboardPageType)source.PageType);
            mapper.AddCustomMapping(src => src.AuthorizationViewPage, dest => dest.AuthorizationViewPage, source =>
                source.AuthorizationViewPage.Split(','),
                condition => !string.IsNullOrEmpty(condition.AuthorizationViewPage));
            mapper.AddCustomMapping(src => src.Settings, dest => dest.Settings, source => TypeCastingUtil<IDictionary<string, object>>.TryJsonCasting(source.Settings));
            mapper.AddCustomMapping(src => src, dest => dest.Module, source => new DashboardPageModuleDto()
            {
                Id = source.ModuleId.Value,
                ModuleType = source.ModuleType,
                ModuleBuilderType = source.ModuleBuilderType,
                ModuleName = source.ModuleName,
                ModuleTitle = source.ModuleTitle
            }, condition => condition.ModuleId != null);

            var result = mapper.Map(page);
            return result;
        }

        public static DashboardPageViewModel MapDashboardPageViewModel(DashboardPageInfo page, DashboardPageModuleView module)
        {
            var mapper = new ExpressionMapper<DashboardPageInfo, DashboardPageViewModel>();
            mapper.AddCustomMapping(src => src.PageType, dest => dest.PageType, source => (DashboardPageType)source.PageType);
            mapper.AddCustomMapping(src => src.AuthorizationViewPage, dest => dest.AuthorizationViewPage,
                source => source.AuthorizationViewPage.Split(','),
                condition => !string.IsNullOrEmpty(condition.AuthorizationViewPage));
            mapper.AddCustomMapping(src => src, dest => dest.Settings, source => TypeCastingUtil<IDictionary<string, object>>.TryJsonCasting(source.Settings));
            mapper.AddCustomMapping(src => src, dest => dest.Module, source => MapDashboardPageModuleViewModel(module), condition => (DashboardPageType)page.PageType == DashboardPageType.Standard && module != null);

            var result = mapper.Map(page);
            return result;
        }

        public static DashboardPageModuleViewModel MapDashboardPageModuleViewModel(DashboardPageModuleView module)
        {
            var mapper = new ExpressionMapper<DashboardPageModuleView, DashboardPageModuleViewModel>();
            return mapper.Map(module);
        }

        public static IEnumerable<DashboardPageModuleDto> MapDashboardPagesModuleDto(IEnumerable<DashboardPageModuleView> modules)
        {
            return modules.Select(module => MapDashboardPageModuleDto(module));
        }

        public static DashboardPageModuleDto MapDashboardPageModuleDto(DashboardPageModuleView module)
        {
            var mapper = new ExpressionMapper<DashboardPageModuleView, DashboardPageModuleDto>();
            return mapper.Map(module);
        }

        public static DashboardInfo MapDashboardInfo(DashboardDto dashboard)
        {
            var mapper = new ExpressionMapper<DashboardDto, DashboardInfo>();
            mapper.AddCustomMapping(src => src.AuthorizationViewDashboard, dest => dest.AuthorizationViewDashboard, source => string.Join(",", source.AuthorizationViewDashboard));
            return mapper.Map(dashboard);
        }

        public static DashboardPageInfo MapDashboardInfo(DashboardPageDto page)
        {
            var mapper = new ExpressionMapper<DashboardPageDto, DashboardPageInfo>();
            mapper.AddCustomMapping(src => src.AuthorizationViewPage, dest => dest.AuthorizationViewPage,
                    source => string.Join(",", source.AuthorizationViewPage),
                    condition => condition.AuthorizationViewPage != null && condition.AuthorizationViewPage.Any());

            mapper.AddCustomMapping(src => src.Settings, dest => dest.Settings,
                source => JsonConvert.SerializeObject(source.Settings),
                condition => condition.Settings != null && condition.Settings?.Count > 0);

            return mapper.Map(page);
        }

        public static IEnumerable<DashboardPageLiteDto> MapDashboardPagesLiteDto(IEnumerable<DashboardPageLiteView> pages)
        {
            return pages.Select(page => MapDashboardPageLiteDto(page));
        }

        public static DashboardPageLiteDto MapDashboardPageLiteDto(DashboardPageLiteView page)
        {
            var mapper = new ExpressionMapper<DashboardPageLiteView, DashboardPageLiteDto>();
            return mapper.Map(page);
        }

        public static IEnumerable<DashboardSkinDto> MapDashboardSkinsDto(IEnumerable<Data.Entities.Tables.DashboardSkinInfo> skins)
        {
            return skins.Select(skin => MapDashboardSkinDto(skin));
        }

        public static DashboardSkinDto MapDashboardSkinDto(Data.Entities.Tables.DashboardSkinInfo skin)
        {
            var mapper = new ExpressionMapper<Data.Entities.Tables.DashboardSkinInfo, DashboardSkinDto>();
            mapper.AddCustomMapping(src => src.SkinImage, dest => dest.SkinImage,
                source => source.SkinImage.Replace("[ModulePath]", "/DesktopModules/BusinessEngine"),
                condition => !string.IsNullOrWhiteSpace((condition.SkinImage)));

            return mapper.Map(skin);
        }

        public static DashboardAppearanceDto MapDashboardAppearanceDto(DashboardAppearanceView skin)
        {
            var mapper = new ExpressionMapper<DashboardAppearanceView, DashboardAppearanceDto>();
            mapper.AddCustomMapping(src => src, dest => dest.DashboardType,
                source => (DashboardType)source.DashboardType);

            return mapper.Map(skin);
        }

        public static IEnumerable<DashboardTemplateDto> MapDashboardTemplatesDto(IEnumerable<DashboardTemplate> templates)
        {
            return templates.Select(template => MapDashboardTemplateDto(template));
        }

        public static DashboardTemplateDto MapDashboardTemplateDto(DashboardTemplate template)
        {
            var mapper = new ExpressionMapper<DashboardTemplate, DashboardTemplateDto>();
            mapper.AddCustomMapping(src => src.TemplateImage, dest => dest.TemplateImage,
                source => source.TemplateImage.Replace("[ModulePath]", "/DesktopModules/BusinessEngine"),
                condition => !string.IsNullOrWhiteSpace((condition.TemplateImage)));

            return mapper.Map(template);
        }

        public static IEnumerable<ModuleCustomLibraryViewModel> MapCustomLibrariesViewModel(IEnumerable<ModuleCustomLibraryView> libraries, IEnumerable<ModuleCustomLibraryResourceView> resources)
        {
            var resourcesDict = resources.GroupBy(r => r.LibraryId)
                                            .ToDictionary(r => r.Key, r => r.AsEnumerable());

            return libraries.Select(library =>
            {
                var libraryResources = resourcesDict.TryGetValue(library.LibraryId, out var items) ? items : Enumerable.Empty<ModuleCustomLibraryResourceView>();
                return MapCustomLibraryViewModel(library, libraryResources);
            });
        }

        public static ModuleCustomLibraryViewModel MapCustomLibraryViewModel(ModuleCustomLibraryView library, IEnumerable<ModuleCustomLibraryResourceView> resources)
        {
            var mapper = new ExpressionMapper<ModuleCustomLibraryView, ModuleCustomLibraryViewModel>();
            mapper.AddCustomMapping(src => src, dest => dest.Resources,
                source => BaseMapping<ModuleCustomLibraryResourceView, ModuleCustomLibraryResourceViewModel>.MapViewModels(resources),
                condition => resources != null && resources.Any());

            return mapper.Map(library);
        }

        #endregion
    }
}