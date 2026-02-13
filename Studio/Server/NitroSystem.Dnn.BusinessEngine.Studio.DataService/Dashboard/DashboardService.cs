using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Data.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Dashboard;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Core.Security;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Views;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Export;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Import;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataService.Dashboard
{
    public class DashboardService : IDashboardService, IExportable, IImportable
    {
        private readonly IRepositoryBase _repository;
        private readonly IUnitOfWork _unitOfWork;

        public DashboardService(
            IUnitOfWork unitOfWork,
            IRepositoryBase repository)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
        }

        #region Dashboard Services

        public async Task<Guid?> GetDashboardIdAsync(Guid moduleId)
        {
            return await _repository.GetColumnValueAsync<DashboardInfo, Guid?>("Id", "ModuleId", moduleId);
        }

        public async Task<DashboardViewModel> GetDashboardViewModelAsync(Guid moduleId)
        {
            var dashboard = await _repository.GetByColumnAsync<DashboardView>("ModuleId", moduleId);

            return HybridMapper.Map<DashboardView, DashboardViewModel>(dashboard);
        }

        public async Task<(Guid, Guid)> SaveDashboardAsync(DashboardViewModel dashboard)
        {
            _unitOfWork.BeginTransaction();

            try
            {
                /*------------------------------------------------------------------------------
                    ----->> Save Module (1) ==> Save Business Engine Module <<-----
                 ------------------------------------------------------------------------------*/
                var objModuleInfo = HybridMapper.Map<DashboardViewModel, ModuleInfo>(dashboard);
                if (dashboard.Id == Guid.Empty)
                {
                    dashboard.ModuleId = await _repository.AddAsync<ModuleInfo>(objModuleInfo);
                }
                else
                {
                    var isUpdated = await _repository.UpdateAsync<ModuleInfo>(objModuleInfo, "ModuleTitle");
                    if (!isUpdated) ErrorHandling.ThrowUpdateFailedException(objModuleInfo);
                }

                /*------------------------------------------------------------------------------
                    ----->> Save Dashboard (2) ==> Save Business Engine Dashboard <<-----
                 ------------------------------------------------------------------------------*/
                var objDashboardInfo = HybridMapper.Map<DashboardViewModel, DashboardInfo>(dashboard);
                if (objDashboardInfo.Id == Guid.Empty)
                {
                    dashboard.Id = await _repository.AddAsync<DashboardInfo>(objDashboardInfo);
                }
                else
                {
                    var isUpdated = await _repository.UpdateAsync<DashboardInfo>(objDashboardInfo, "AuthorizationViewDashboard");
                    if (!isUpdated) ErrorHandling.ThrowUpdateFailedException(objDashboardInfo);
                }

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
            return await _repository.DeleteAsync<DashboardInfo>(id);
        }

        #endregion

        #region Dashboard Pages

        public async Task<IEnumerable<DashboardPageViewModel>> GetDashboardPagesViewModelAsync(Guid dashboardModuleId)
        {
            var dashboardId = await GetDashboardIdAsync(dashboardModuleId);
            var pages = await _repository.GetByScopeAsync<DashboardPageInfo>(dashboardId.Value);

            return await BuildPageTree(pages);
        }

        public async Task<IEnumerable<DashboardPageListItem>> GetDashboardPagesListItemAsync(Guid dashboardModuleId)
        {
            var dashboardId = await GetDashboardIdAsync(dashboardModuleId);
            var pages = await _repository.GetItemsByColumnAsync<DashboardPageInfo>("DashboardId", dashboardId, "PageName");

            return HybridMapper.MapCollection<DashboardPageInfo, DashboardPageListItem>(pages);
        }

        public async Task<DashboardPageViewModel> GetDashboardPageViewModelAsync(Guid pageId)
        {
            var page = await _repository.GetAsync<DashboardPageInfo>(pageId);

            return await HybridMapper.MapAsync<DashboardPageInfo, DashboardPageViewModel>(
               source: page,
               configAction: async (src, dest) =>
               {
                   if (dest.PageType == DashboardPageType.Standard && dest.IncludeModule)
                   {
                       var module = await _repository.GetByColumnAsync<DashboardPageModuleView>("PageId", src.Id);
                       dest.Module = HybridMapper.Map<DashboardPageModuleView, DashboardPageModuleViewModel>(module);
                   }
               }
            );
        }

        public async Task<(Guid?, Guid?, Guid?)> SaveDashboardPageAsync(DashboardPageViewModel page)
        {
            var ids = new Guid?[3];

            _unitOfWork.BeginTransaction();

            try
            {
                var objDashboardPageInfo = HybridMapper.Map<DashboardPageViewModel, DashboardPageInfo>(page);
                if (page.Id == Guid.Empty)
                {
                    objDashboardPageInfo.ViewOrder = await _repository.ExecuteStoredProcedureScalerAsync<int?>(
                        "dbo.BusinessEngine_Studio_GetLastDashboardPage", "BE_Dashboards_Pages_Studio_GetLastDashboardPage_",
                        new { objDashboardPageInfo.ParentId }) ?? 1;
                    ids[0] = objDashboardPageInfo.Id = await _repository.AddAsync<DashboardPageInfo>(objDashboardPageInfo);
                }
                else
                {
                    ids[0] = page.Id;

                    var isUpdated = await _repository.UpdateAsync<DashboardPageInfo>(objDashboardPageInfo);
                    if (!isUpdated) ErrorHandling.ThrowUpdateFailedException(objDashboardPageInfo);
                }

                if (page.PageType == DashboardPageType.Standard)
                {
                    if (!page.IncludeModule && page.Module != null && page.Module.Id != Guid.Empty)
                    {
                        await _repository.DeleteAsync<ModuleInfo>(page.Module.Id);
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
                            ids[1] = objModuleInfo.Id = await _repository.AddAsync<ModuleInfo>(objModuleInfo);
                        }
                        else
                        {
                            ids[1] = objModuleInfo.Id;

                            var isUpdated = await _repository.UpdateAsync<ModuleInfo>(objModuleInfo, "ModuleType", "ModuleName", "ModuleTitle");
                            if (!isUpdated) ErrorHandling.ThrowUpdateFailedException(objModuleInfo);
                        }

                        var objDashboardPageModuleInfo = HybridMapper.Map<DashboardPageModuleViewModel, DashboardPageModuleInfo>(page.Module);
                        if (objDashboardPageModuleInfo.Id == Guid.Empty)
                        {
                            objDashboardPageModuleInfo.PageId = objDashboardPageInfo.Id;
                            objDashboardPageModuleInfo.ModuleId = objModuleInfo.Id;
                            ids[2] = objDashboardPageModuleInfo.Id = await _repository.AddAsync<DashboardPageModuleInfo>(objDashboardPageModuleInfo);
                        }
                        else
                        {
                            ids[2] = objDashboardPageModuleInfo.Id;

                            var isUpdated = await _repository.UpdateAsync<DashboardPageModuleInfo>(objDashboardPageModuleInfo);
                            if (!isUpdated) ErrorHandling.ThrowUpdateFailedException(objDashboardPageModuleInfo);
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

        public async Task SortDashboardPages(DashboardPagesOrder dashboard)
        {
            _unitOfWork.BeginTransaction();

            try
            {
                int index = 0;
                foreach (var pageId in dashboard.SortedPageIds)
                {
                    var objDashboardPageInfo = new DashboardPageInfo() { Id = pageId, ViewOrder = index++ };
                    await _repository.UpdateAsync<DashboardPageInfo>(objDashboardPageInfo, "ViewOrder");
                }

                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();

                throw ex;
            }
        }

        public async Task UpdatePageParent(UpdateDashboardPageParent page)
        {
            var objDashboardPageInfo = new DashboardPageInfo() { Id = page.Id, ParentId = page.ParentId };
            await _repository.UpdateAsync<DashboardPageInfo>(objDashboardPageInfo, "ParentId");
        }

        public async Task<bool> DeletePageAsync(Guid id)
        {
            return await _repository.DeleteAsync<DashboardPageInfo>(id);
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
                       var module = await _repository.GetByColumnAsync<DashboardPageModuleView>("PageId", src.Id);
                       dest.Module = HybridMapper.Map<DashboardPageModuleView, DashboardPageModuleViewModel>(module);
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
                    var module = await GetDashboarsdAndPagesAsync(context.Get<Guid>("ScenarioId"));

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
            var dashboards = JsonConvert.DeserializeObject<IEnumerable<DashboardInfo>>(items[0].ToString());
            var dashboardsPages = JsonConvert.DeserializeObject<IEnumerable<DashboardPageInfo>>(items[1].ToString());
            var dashboardsPagesModules = JsonConvert.DeserializeObject<IEnumerable<DashboardPageModuleInfo>>(items[2].ToString());

            if (context.Scope == ImportExportScope.ScenarioFullComponents)
            {
                await BulkInsertDashboardsAndPagesAsync(dashboards, dashboardsPages, dashboardsPagesModules);
            }

            return new ImportResponse()
            {
                IsSuccess = true
            };
        }

        private async Task<object> GetDashboarsdAndPagesAsync(Guid scenarioId)
        {
            var modules = await _repository.GetItemsByColumnsAsync<ModuleInfo>(
                new string[2] { "ScenarioId", "ModuleType" },
                new
                {
                    ScenarioId = scenarioId,
                    ModuleType = 0
                }
            );

            var dashboards = new List<DashboardInfo>();
            var dashboardsPages = new List<DashboardPageInfo>();
            var dashboardsPagesModules = new List<DashboardPageModuleInfo>();

            foreach (var module in modules)
            {
                var dashboard = await _repository.GetByColumnAsync<DashboardInfo>("ModuleId", module.Id);
                dashboards.Add(dashboard);

                var pages = await _repository.GetByScopeAsync<DashboardPageInfo>(dashboard.Id);
                dashboardsPages.AddRange(pages);

                foreach (var page in pages)
                {
                    var pageModules = await _repository.GetByScopeAsync<DashboardPageModuleInfo>(page.Id);
                    dashboardsPagesModules.AddRange(pageModules);
                }
            }

            return new List<object>() { dashboards, dashboardsPages, dashboardsPagesModules };
        }

        private async Task BulkInsertDashboardsAndPagesAsync(IEnumerable<DashboardInfo> dashboards, IEnumerable<DashboardPageInfo> dashboardsPages, IEnumerable<DashboardPageModuleInfo> dashboardsPagesModules)
        {
            await _repository.BulkInsertAsync<DashboardInfo>(dashboards);
            await _repository.BulkInsertAsync<DashboardPageInfo>(dashboardsPages);
            await _repository.BulkInsertAsync<DashboardPageModuleInfo>(dashboardsPagesModules);
        }

        #endregion
    }
}