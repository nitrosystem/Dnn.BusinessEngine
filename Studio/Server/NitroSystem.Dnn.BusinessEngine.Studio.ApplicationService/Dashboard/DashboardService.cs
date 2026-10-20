using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Dashboard;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Dashboard;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Views.Dashboard;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Dashboard;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Export;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Import;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;

namespace NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.Dashboard
{
    public class DashboardService : IDashboardService, IExportable, IImportable
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IModuleService _moduleService;
        private readonly IModuleRepository _moduleRepository;
        private readonly IDashboardRepository _dashboardRepository;
        private readonly IDashboardPageRepository _dashboardPageRepository;
        private readonly IDashboardPageModuleRepository _dashboardPageModuleRepository;

        public DashboardService(
            IUnitOfWork unitOfWork,
            IModuleService moduleService,
            IModuleRepository moduleRepository,
            IDashboardRepository dashboardRepository,
            IDashboardPageRepository dashboardPageRepository,
            IDashboardPageModuleRepository dashboardPageModuleRepository)
        {
            _unitOfWork = unitOfWork;
            _moduleService = moduleService;
            _moduleRepository = moduleRepository;
            _dashboardRepository = dashboardRepository;
            _dashboardPageRepository = dashboardPageRepository;
            _dashboardPageModuleRepository = dashboardPageModuleRepository;
        }

        #region Dashboard Services

        public async Task<Guid> GetDashboardIdAsync(Guid moduleId)
        {
            return await _dashboardRepository.GetIdByModuleIdAsync(moduleId);
        }

        public async Task<DashboardViewModel> GetDashboardViewModelAsync(Guid moduleId)
        {
            var dashboard = await _dashboardRepository.GetViewByModuleIdAsync(moduleId);
            return HybridMapper.Map<DashboardView, DashboardViewModel>(dashboard);
        }

        public async Task<(Guid DashboardId, Guid ModuleId)> CreateDashboardAsync(DashboardViewModel dashboard)
        {
            _unitOfWork.BeginTransaction();

            try
            {
                /*------------------------------------------------------------------------------
                    ----->> Save Module (1) ==> Save Business Engine Module <<-----
                 ------------------------------------------------------------------------------*/
                var objModuleInfo = HybridMapper.Map<DashboardViewModel, ModuleInfo>(dashboard, (src, desc) => desc.Id = src.ModuleId);
                if (objModuleInfo.Id == Guid.Empty)
                    dashboard.ModuleId = await _moduleRepository.AddAsync(objModuleInfo);
                else
                    await _moduleRepository.UpdateAsync(objModuleInfo, "ModuleTitle");

                /*------------------------------------------------------------------------------
                    ----->> Save Dashboard (2) ==> Save Business Engine Dashboard <<-----
                 ------------------------------------------------------------------------------*/
                var objDashboardInfo = HybridMapper.Map<DashboardViewModel, DashboardInfo>(dashboard);
                if (objDashboardInfo.Id == Guid.Empty)
                    dashboard.Id = await _dashboardRepository.AddAsync(objDashboardInfo);
                else
                    await _dashboardRepository.UpdateAuthorizationViewDashboardAsync(objDashboardInfo);

                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                throw ex;
            }

            return (dashboard.Id, dashboard.ModuleId);
        }

        public async Task<bool> DeleteDashboardAsync(Guid id)
        {
            return await _dashboardRepository.DeleteAsync(id);
        }

        #endregion

        #region Dashboard Pages

        public async Task<IEnumerable<DashboardPageViewModel>> GetDashboardPagesViewModelAsync(Guid dashboardModuleId)
        {
            var dashboardId = await GetDashboardIdAsync(dashboardModuleId);
            var pages = await _dashboardPageRepository.GetsAsync(dashboardId, "ViewOrder");

            return await BuildPageTree(pages);
        }

        public async Task<IEnumerable<DashboardPageListItem>> GetDashboardPagesListItemAsync(Guid dashboardModuleId)
        {
            var dashboardId = await GetDashboardIdAsync(dashboardModuleId);
            var pages = await _dashboardPageRepository.GetsAsync(dashboardId, "ViewOrder");

            return HybridMapper.MapCollection<DashboardPageInfo, DashboardPageListItem>(pages);
        }

        public async Task<DashboardPageViewModel> GetDashboardPageViewModelAsync(Guid pageId)
        {
            var page = await _dashboardPageRepository.GetAsync(pageId);
            return await HybridMapper.MapAsync<DashboardPageInfo, DashboardPageViewModel>(
               source: page,
               configAction: async (src, dest) =>
               {
                   if (dest.PageType == DashboardPageType.Standard && dest.IncludeModule)
                   {
                       var module = (await _dashboardPageModuleRepository.GetsViewAsync(src.Id))?.First();
                       if (module != null)
                           dest.Module = HybridMapper.Map<DashboardPageModuleView, DashboardPageModuleViewModel>(module);
                   }
               }
            );
        }

        public async Task<(Guid?, Guid?, Guid?)> CreateDashboardPageAsync(DashboardPageViewModel page, int userId, string basePath)
        {
            var ids = new Guid?[3];

            _unitOfWork.BeginTransaction();

            try
            {
                var objDashboardPageInfo = HybridMapper.Map<DashboardPageViewModel, DashboardPageInfo>(page);
                if (objDashboardPageInfo.Id == Guid.Empty)
                {
                    ids[0] = objDashboardPageInfo.Id = await _dashboardPageRepository.AddAsync(objDashboardPageInfo);
                }
                else
                {
                    ids[0] = page.Id;
                    await _dashboardPageRepository.UpdateAsync(objDashboardPageInfo);
                }

                if (page.PageType == DashboardPageType.Standard)
                {
                    if (!page.IncludeModule && page.Module != null && page.Module.Id != Guid.Empty)
                    {
                        await _moduleRepository.DeleteAsync(page.Module.Id);
                    }
                    else if (page.IncludeModule)
                    {
                        var objModuleInfo = HybridMapper.Map<DashboardPageModuleViewModel, ModuleInfo>(
                            source: page.Module,
                            configAction: (src, dest) =>
                            {
                                dest.Id = src.ModuleId;
                            }
                        );

                        if (objModuleInfo.Id == Guid.Empty)
                        {
                            ids[1] = objModuleInfo.Id = await _moduleRepository.AddAsync(objModuleInfo);
                        }
                        else
                        {
                            ids[1] = objModuleInfo.Id;

                            var oldModule = await _moduleRepository.GetViewAsync(objModuleInfo.Id);

                            await _moduleRepository.UpdateAsync(objModuleInfo, "ModuleType", "ModuleName", "ModuleTitle");

                            if (oldModule?.ModuleName != objModuleInfo.ModuleName)
                            {
                                _moduleService.RenameModuleFolder(oldModule.ModuleName, objModuleInfo.ModuleName, oldModule.ParentModuleName, oldModule.ScenarioName, basePath);
                                await _moduleService.EnqueueRebuildIfRequiredAsync(objModuleInfo.ScenarioId, objModuleInfo.Id, userId, basePath);
                            }

                        }

                        var objDashboardPageModuleInfo = HybridMapper.Map<DashboardPageModuleViewModel, DashboardPageModuleInfo>(page.Module);
                        if (objDashboardPageModuleInfo.Id == Guid.Empty)
                        {
                            objDashboardPageModuleInfo.PageId = objDashboardPageInfo.Id;
                            objDashboardPageModuleInfo.ModuleId = objModuleInfo.Id;
                            ids[2] = objDashboardPageModuleInfo.Id = await _dashboardPageModuleRepository.AddAsync(objDashboardPageModuleInfo);
                        }
                        else
                        {
                            ids[2] = objDashboardPageModuleInfo.Id;
                            await _dashboardPageModuleRepository.UpdateAsync(objDashboardPageModuleInfo);
                        }
                    }
                }

                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                throw ex;
            }

            return (ids[0], ids[1], ids[2]);
        }

        public async Task SortDashboardPagesAsync(ItemsOrder dashboard)
        {
            _unitOfWork.BeginTransaction();

            try
            {
                int index = 0;
                foreach (var pageId in dashboard.SortedIds)
                {
                    var objDashboardPageInfo = new DashboardPageInfo() { Id = pageId, ViewOrder = index++ };
                    await _dashboardPageRepository.UpdateAsync(objDashboardPageInfo, "ViewOrder");
                }

                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                throw ex;
            }
        }

        public async Task UpdatePageParentAsync(UpdateDashboardPageParent page)
        {
            var objDashboardPageInfo = new DashboardPageInfo() { Id = page.Id, ParentId = page.ParentId };
            await _dashboardPageRepository.UpdateAsync(objDashboardPageInfo, "ParentId");
        }

        public async Task<bool> DeletePageAsync(Guid pageId)
        {
            return await _dashboardPageRepository.DeleteAsync(pageId);
        }

        private async Task<IEnumerable<DashboardPageViewModel>> BuildPageTree(IEnumerable<DashboardPageInfo> pages)
        {
            var pageLookup = pages.ToLookup(p => p.ParentId);
            return await PopulateDashboardPages(null, pageLookup);
        }

        private async Task<IEnumerable<DashboardPageViewModel>> PopulateDashboardPages(Guid? parentId, ILookup<Guid?, DashboardPageInfo> lookup)
        {
            return await HybridMapper.MapCollectionAsync<DashboardPageInfo, DashboardPageViewModel>(
               sources: lookup[parentId],
               configAction: async (src, dest) =>
               {
                   if (dest.PageType == DashboardPageType.Standard && dest.IncludeModule)
                   {
                       var modules = await _dashboardPageModuleRepository.GetsViewAsync(src.Id);
                       if (modules != null && modules.Any())
                           dest.Module = HybridMapper.Map<DashboardPageModuleView, DashboardPageModuleViewModel>(modules.First());
                   }

                   dest.Pages = await PopulateDashboardPages(src.Id, lookup);
               }
            );
        }

        #endregion

        #region Import Export

        public async Task<ExportResponse> ExportAsync(ExportContext context)
        {
            switch (context.Scope)
            {
                case ImportExportScope.ScenarioFullComponents:
                    var module = await GetDashboarsdAndPagesAndModulesAsync(context.Get<Guid>("ScenarioId"));

                    return new ExportResponse()
                    {
                        Result = module,
                        IsSuccess = true
                    };
                default:
                    return null;
            }
        }

        public async Task<ImportResponse> ImportAsync(string json, ImportContext context)
        {
            var items = JsonConvert.DeserializeObject<List<object>>(json);
            var dashboards = JsonConvert.DeserializeObject<IReadOnlyList<DashboardInfo>>(items[0].ToString());
            var dashboardsPages = JsonConvert.DeserializeObject<IReadOnlyList<DashboardPageInfo>>(items[1].ToString());
            var dashboardsPagesModules = JsonConvert.DeserializeObject<IReadOnlyList<DashboardPageModuleInfo>>(items[2].ToString());

            if (context.Scope == ImportExportScope.ScenarioFullComponents)
            {
                await BulkInsertDashboardsAndPagesAndModulesAsync(dashboards, dashboardsPages, dashboardsPagesModules);
            }

            return new ImportResponse()
            {
                IsSuccess = true
            };
        }

        private async Task<object> GetDashboarsdAndPagesAndModulesAsync(Guid scenarioId)
        {
            var dashboards = await _dashboardRepository.GetsByAncestorAsync(scenarioId);
            var dashboardsPages = new List<DashboardPageInfo>();
            var dashboardsPagesModules = new List<DashboardPageModuleInfo>();

            foreach (var dashboard in dashboards)
            {
                var pages = await _dashboardPageRepository.GetsAsync(dashboard.Id);
                foreach (var page in pages)
                {
                    var pageModules = await _dashboardPageModuleRepository.GetsByAncestorAsync(page.Id);
                    dashboardsPagesModules.AddRange(pageModules);
                }

                dashboardsPages.AddRange(pages);
            }

            return new List<object>() { dashboards, dashboardsPages, dashboardsPagesModules };
        }

        private async Task BulkInsertDashboardsAndPagesAndModulesAsync(IReadOnlyList<DashboardInfo> dashboards, IReadOnlyList<DashboardPageInfo> dashboardsPages, IReadOnlyList<DashboardPageModuleInfo> dashboardsPagesModules)
        {
            await _dashboardRepository.BulkInsertAsync(dashboards);
            await _dashboardPageRepository.BulkInsertAsync(dashboardsPages);
            await _dashboardPageModuleRepository.BulkInsertAsync(dashboardsPagesModules);
        }

        #endregion
    }
}