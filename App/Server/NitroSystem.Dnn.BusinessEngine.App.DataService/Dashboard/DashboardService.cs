using System;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Data.Contracts;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Dto;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Contracts;
using System.Collections.Generic;
using System.Linq;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Procedures;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Web.Dto;

namespace NitroSystem.Dnn.BusinessEngine.App.DataService.Module
{
    public class DashboardService : IDashboardService
    {
        private readonly IRepositoryBase _repository;

        public DashboardService(IRepositoryBase repository)
        {
            _repository = repository;
        }

        public async Task<DashboardDto> GetDashboardDtoAsync(Guid moduleId, bool isSuperUser, string[] userRoles)
        {
            var dashboard = await _repository.GetByColumnAsync<DashboardInfo>("ModuleId", moduleId);
            var pages = await _repository.GetByScopeAsync<DashboardPageInfo>(dashboard.Id, "ViewOrder");

            return HybridMapper.Map<DashboardInfo, DashboardDto>(
                source: dashboard,
                configAction: (src, dest) =>
                {
                    dest.Pages = BuildPageTree(pages, isSuperUser, userRoles);
                }
            );
        }

        public DashboardPageModuleDto GetDashboardPageModule(Guid dashboardModuleId, string pageName)
        {
            var module = _repository.ExecuteStoredProcedure<DashboardPageModuleSpResult>(
                "dbo.BusinessEngine_App_GetDashboardPageModule", "Be_Modules_DashboardPageModule",
                new
                {
                    DashboardModuleId = dashboardModuleId,
                    PageName = pageName
                });

            return HybridMapper.Map<DashboardPageModuleSpResult, DashboardPageModuleDto>(module);
        }

        private IEnumerable<DashboardPageDto> BuildPageTree(IEnumerable<DashboardPageInfo> pages, bool isSuperUser, string[] userRoles)
        {
            var pageLookup = pages.Where(p => isSuperUser ||
                                        p.InheritPermissionFromDashboard ||
                                        string.IsNullOrEmpty(p.AuthorizationViewPage) ||
                                        p.AuthorizationViewPage.Split(',').Any(p => userRoles.Contains(p)))
                                  .ToLookup(p => p.ParentId);

            return PopulateDashboardPages(null, pageLookup);
        }

        private IEnumerable<DashboardPageDto> PopulateDashboardPages(Guid? parentId, ILookup<Guid?, DashboardPageInfo> lookup)
        {
            return HybridMapper.MapCollection<DashboardPageInfo, DashboardPageDto>(
               sources: lookup[parentId],
               configAction: (src, dest) =>
               {
                   dest.Pages = PopulateDashboardPages(src.Id, lookup);
               }
            );
        }
    }
}
