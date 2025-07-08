using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Web;
using NitroSystem.Dnn.BusinessEngine.Common.Reflection;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Dashboard.Skin;
using NitroSystem.Dnn.BusinessEngine.Core.Cashing;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Core.Providers;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Dto;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Enums;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Mapping;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Dashboard;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Repository;
using NitroSystem.Dnn.BusinessEngine.Utilities;
using DotNetNuke.UI.Skins;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.Enums;
using System.Web.UI;
using System.Reflection;
using NitroSystem.Dnn.BusinessEngine.Core.Mapper;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.DashboardAppearance.Models;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.DashboardAppearance;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Services
{
    public class DashboardService : IDashbaordService
    {
        private readonly ICacheService _cacheService;
        private readonly IModuleService _moduleService;
        private readonly IRepositoryBase _repository;
        private readonly IUnitOfWork _unitOfWork;

        public DashboardService(IUnitOfWork unitOfWork, ICacheService cacheService, IModuleService moduleService)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _repository = new RepositoryBase(_unitOfWork, _cacheService);
            _moduleService = moduleService;
        }

        #region Dashboard Basic Info

        public async Task<Guid?> GetDashboardIdAsync(Guid moduleId)
        {
            return await _repository.GetColumnValueAsync<DashboardInfo, Guid?>("Id", "ModuleId", moduleId);
        }

        public async Task<DashboardDto> GetDashboardDtoAsync(Guid moduleId)
        {
            var dashboardView = await _repository.GetByColumnAsync<DashboardView>("ModuleId", moduleId);

            return DashboardMapping.MapDashboardDto(dashboardView);
        }

        public async Task<(Guid, Guid)> SaveDashboardBasicInfoAsync(DashboardDto dashboard)
        {
            var userId = CurrentUserProvider.GetCurrentUserId(_cacheService);

            /*------------------------------------------------------------------------------
                ----->> Save Module (1) ==> Save Business Engine Module <<-----
             ------------------------------------------------------------------------------*/
            var objModuleInfo = new ModuleInfo();
            PropertyCopier<DashboardDto, ModuleInfo>.Copy(dashboard, objModuleInfo);
            objModuleInfo.Id = dashboard.ModuleId;
            objModuleInfo.Wrapper = (int)ModuleWrapper.DnnPage;
            objModuleInfo.ModuleType = (int)ModuleType.Dashboard;
            objModuleInfo.ModuleBuilderType = (int)ModuleBuilderType.FormDesigner;
            if (objModuleInfo.Id == Guid.Empty)
            {
                objModuleInfo.Id = await _repository.AddAsync<ModuleInfo>(objModuleInfo);
            }
            else
            {
                var isUpdated = await _repository.UpdateAsync<ModuleInfo>(objModuleInfo, "ModuleTitle");
                if (!isUpdated) ErrorService.ThrowUpdateFailedException(objModuleInfo);
            }

            /*------------------------------------------------------------------------------
                ----->> Save Dashboard (2) ==> Save Business Engine Dashboard <<-----
             ------------------------------------------------------------------------------*/
            var objDashboardInfo = DashboardMapping.MapDashboardInfo(dashboard);
            if (objModuleInfo.Id == Guid.Empty)
            {
                objDashboardInfo.Id = await _repository.AddAsync<DashboardInfo>(objDashboardInfo);
            }
            else
            {
                var isUpdated = await _repository.UpdateAsync<DashboardInfo>(objDashboardInfo,
                    "DashboardType",
                    "UniqueName",
                    "AuthorizationViewDashboard"
                );
                if (!isUpdated) ErrorService.ThrowUpdateFailedException(objDashboardInfo);
            }

            return (objDashboardInfo.Id, objModuleInfo.Id);
        }

        public async Task<bool> DeleteDashboardAsync(Guid id)
        {
            return await _repository.DeleteAsync<DashboardInfo>(id);
        }

        #endregion

        #region Dashboard Pages

        public async Task<IEnumerable<DashboardPageDto>> GetDashboardPagesViewModelAsync(Guid moduleId)
        {
            var dashboardId = await GetDashboardIdAsync(moduleId);
            var pages = await _repository.GetByScopeAsync<DashboardPageInfo>(dashboardId.Value);

            return DashboardMapping.BuildPageTree(pages);
        }

        public async Task<DashboardPageDto> GetDashboardPageDtoAsync(Guid pageId)
        {
            var page = await _repository.GetAsync<DashboardPageView>(pageId);

            return DashboardMapping.MapDashboardPageDto(page);
        }

        public async Task<IEnumerable<DashboardPageLiteDto>> GetDashboardPagesLiteDtoAsync(Guid dashboardModuleId)
        {
            var pages = await _repository.GetByScopeAsync<DashboardPageLiteView>(dashboardModuleId);

            return DashboardMapping.MapDashboardPagesLiteDto(pages);
        }

        public async Task<(Guid?, Guid?, Guid?)> SaveDashboardPageAsync(DashboardPageDto page)
        {
            var userId = CurrentUserProvider.GetCurrentUserId(_cacheService);
            var ids = new Guid?[3];

            /*------------------------------------------------------------------------------
                ----->> Save Page (1) ==> Save Business Engine Dashboard Page <<-----
             ------------------------------------------------------------------------------*/
            var objDashboardPageInfo = DashboardMapping.MapDashboardInfo(page);
            if (page.Id == Guid.Empty)
            {
                objDashboardPageInfo.ViewOrder = await _repository.ExecuteStoredProcedureScalerAsync<int?>(
                    "BusinessEngine_GetLastDashboardPage",
                    new { objDashboardPageInfo.ParentId }) ?? 1;
                ids[0] = objDashboardPageInfo.Id = await _repository.AddAsync(objDashboardPageInfo);
            }
            else
            {
                ids[0] = page.Id;

                var isUpdated = await _repository.UpdateAsync(objDashboardPageInfo);
                if (!isUpdated) ErrorService.ThrowUpdateFailedException(objDashboardPageInfo);
            }

            if (page.PageType == DashboardPageType.Standard)
            {
                /*------------------------------------------------------------------------------
                        ----->> Save Module (2) ==> Save Business Engine Module <<-----
                 ------------------------------------------------------------------------------*/
                var objModuleInfo = new ModuleInfo();
                PropertyCopier<DashboardPageModuleDto, ModuleInfo>.Copy(page.Module, objModuleInfo);
                objModuleInfo.Wrapper = (int)ModuleWrapper.Dashboard;
                objModuleInfo.ParentId = page.DashboardModuleId;
                objModuleInfo.ScenarioId = page.ScenarioId;
                if (objModuleInfo.Id == Guid.Empty)
                {
                    ids[1] = objModuleInfo.Id = await _repository.AddAsync(objModuleInfo);
                }
                else
                {
                    ids[1] = objModuleInfo.Id;

                    var isUpdated = await _repository.UpdateAsync(objModuleInfo);
                    if (!isUpdated) ErrorService.ThrowUpdateFailedException(objModuleInfo);
                }

                /*------------------------------------------------------------------------------
                    ----->> Save Page Module (3) ==> Save Business Engine Dashboard Page Module <<-----
                 ------------------------------------------------------------------------------*/
                var objDashboardPageModuleInfo = new DashboardPageModuleInfo();
                objDashboardPageModuleInfo.Id = page.PageModuleId ?? Guid.Empty;
                objDashboardPageModuleInfo.PageId = objDashboardPageInfo.Id;
                objDashboardPageModuleInfo.ModuleId = objModuleInfo.Id;
                objDashboardPageModuleInfo.ViewOrder = 1;
                if (objDashboardPageModuleInfo.Id == Guid.Empty)
                {
                    ids[2] = objDashboardPageModuleInfo.Id =
                        await _repository.AddAsync(objDashboardPageModuleInfo);
                }
                else
                {
                    ids[2] = objDashboardPageModuleInfo.Id;

                    var isUpdated = await _repository.UpdateAsync(objDashboardPageModuleInfo);
                    if (!isUpdated) ErrorService.ThrowUpdateFailedException(objDashboardPageModuleInfo);
                }
            }

            return (ids[0], ids[1], ids[2]);
        }

        #endregion

        #region Dashboard Appearance

        public async Task<(DashboardAppearanceDto, IEnumerable<DashboardSkinDto>, IEnumerable<DashboardTemplateDto>)> GetDashboardAppearanceAsync(
            Guid moduleId, HttpContext context)
        {
            var results = await _repository.ExecuteStoredProcedureMultiGridResultAsync(
                "BusinessEngine_GetDashboardAppearance",
                new { ModuleId = moduleId },
                grid => grid.ReadFirstOrDefault<DashboardAppearanceView>(),
                grid => grid.Read<DashboardSkinInfo>()
            );

            var dashboard = results[0] as DashboardAppearanceView;
            var skins = results[1] as IEnumerable<DashboardSkinInfo> ?? Enumerable.Empty<DashboardSkinInfo>();

            var templates = dashboard.SkinId.HasValue
                ? DashboardSkinManager.GetDashboardTemplates(
                    (DashboardType)dashboard.DashboardType, dashboard.Skin, dashboard.SkinPath, context)
                : Enumerable.Empty<DashboardTemplate>();

            return
            (
                DashboardMapping.MapDashboardAppearanceDto(dashboard),
                DashboardMapping.MapDashboardSkinsDto(skins),
                DashboardMapping.MapDashboardTemplatesDto(templates)
            );
        }

        public async Task<IEnumerable<ModuleTemplateViewModel>> GetModuleTemplates(Guid dashboardModuleId, ModuleType moduleType, HttpContext context)
        {
            var dashboard = (await _repository.GetAllAsync<DashboardAppearanceView>())?.FirstOrDefault(d => d.ModuleId == dashboardModuleId);

            var templates = DashboardSkinManager.GetModuleTemplates(
                (DashboardType)dashboard.DashboardType,
                moduleType,
                dashboard.Skin,
                dashboard.SkinPath,
                context
            );

            return templates.Select(source =>
            {
                return HybridMapper.MapWithConfig<ModuleTemplate, ModuleTemplateViewModel>(
                    source, (src, dest) =>
                    {
                        dest.TemplateImage = dest.TemplateImage.ReplaceFrequentTokens();
                        dest.TemplatePath = dest.TemplatePath.ReplaceFrequentTokens();
                    });
            });
        }

        public async Task<IEnumerable<DashboardTemplateDto>> GetDashboardTemplatesDtoAsync(DashboardType dashboardType, string skinName, string skinPath, HttpContext context)
        {
            var templates = DashboardSkinManager.GetDashboardTemplates(dashboardType, skinName, skinPath, context);
            return DashboardMapping.MapDashboardTemplatesDto(templates);
        }

        public async Task SaveDashboardAppearanceAsync(DashboardAppearanceDto dashboard, HttpContext context)
        {
            var objDashboardInfo = new DashboardInfo();
            PropertyCopier<DashboardAppearanceDto, DashboardInfo>.Copy(dashboard, objDashboardInfo);

            string layoutTemplate = DashboardSkinManager.GetDashboardTemplateHtml(dashboard.DashboardType, dashboard.Skin, dashboard.SkinPath, dashboard.Template, context);

            var objModuleInfo = new ModuleInfo();
            objModuleInfo.Id = dashboard.ModuleId;
            objModuleInfo.Template = dashboard.Template;
            objModuleInfo.LayoutTemplate = layoutTemplate;

            var task1 = _repository.UpdateAsync<DashboardInfo>(objDashboardInfo, "SkinId");
            var task2 = _repository.UpdateAsync<ModuleInfo>(objModuleInfo, "Template", "LayoutTemplate");

            await Task.WhenAll(task1, task2);
        }

        #endregion

        #region Dashboard Modules

        public async Task<IEnumerable<DashboardPageModuleDto>> GetDashboardPagesModule(Guid moduleId)
        {
            var modules = await _repository.GetByScopeAsync<DashboardPageModuleView>(moduleId);

            return DashboardMapping.MapDashboardPagesModuleDto(modules);
        }

        #endregion

        #region Dashboard Libraries

        public async Task<IEnumerable<ModuleCustomLibraryViewModel>> GetDashboardLibraries(Guid moduleId)
        {
            var task1 = _repository.GetByScopeAsync<ModuleCustomLibraryView>(moduleId);
            var task2 = _repository.GetByScopeAsync<ModuleCustomLibraryResourceView>(moduleId);

            await Task.WhenAll(task1, task2);

            return DashboardMapping.MapCustomLibrariesViewModel(await task1, await task2);
        }

        public async Task<IEnumerable<ModuleCustomResourceViewModel>> GetDashboardResources(Guid moduleId)
        {
            var resources = await _repository.GetByScopeAsync<ModuleCustomResourceInfo>(moduleId);

            return BaseMapping<ModuleCustomResourceInfo, ModuleCustomResourceViewModel>.MapViewModels(resources);
        }

        public async Task<Guid> SaveCustomLibrary(ModuleCustomLibraryDto library)
        {
            var objModuleCustomLibraryInfo = new ModuleCustomLibraryInfo();
            PropertyCopier<ModuleCustomLibraryDto, ModuleCustomLibraryInfo>.Copy(library, objModuleCustomLibraryInfo);

            if (library.Id == Guid.Empty)
                objModuleCustomLibraryInfo.Id = await _repository.AddAsync<ModuleCustomLibraryInfo>(objModuleCustomLibraryInfo);
            else
            {
                var isUpdated = await _repository.UpdateAsync<ModuleCustomLibraryInfo>(objModuleCustomLibraryInfo);
                if (!isUpdated) ErrorService.ThrowUpdateFailedException(objModuleCustomLibraryInfo);
            }

            return objModuleCustomLibraryInfo.Id;
        }

        public async Task<Guid> SaveCustomResource(ModuleCustomResourceDto resource)
        {
            var objModuleCustomResourceInfo = new ModuleCustomResourceInfo();
            PropertyCopier<ModuleCustomResourceDto, ModuleCustomResourceInfo>.Copy(resource, objModuleCustomResourceInfo);

            if (resource.Id == Guid.Empty)
                objModuleCustomResourceInfo.Id = await _repository.AddAsync<ModuleCustomResourceInfo>(objModuleCustomResourceInfo);
            else
            {
                var isUpdated = await _repository.UpdateAsync<ModuleCustomResourceInfo>(objModuleCustomResourceInfo);
                if (!isUpdated) ErrorService.ThrowUpdateFailedException(objModuleCustomResourceInfo);
            }

            return objModuleCustomResourceInfo.Id;
        }

        public async Task<bool> DeleteLibraryAsync(Guid id)
        {
            return await _repository.DeleteAsync<ModuleCustomLibraryInfo>(id);
        }

        public async Task<bool> DeleteResourceAsync(Guid id)
        {
            return await _repository.DeleteAsync<ModuleCustomResourceInfo>(id);
        }

        #endregion
    }
}