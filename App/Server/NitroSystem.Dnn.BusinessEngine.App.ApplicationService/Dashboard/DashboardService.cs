using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Dashboard;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Dashboard;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Procedures.Dashboard;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Web.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;

namespace NitroSystem.Dnn.BusinessEngine.App.ApplicationService.Dashboard
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepository;
        private readonly IDashboardPageRepository _dashboardPageRepository;
        private readonly IDashboardPageModuleRepository _dashboardPageModuleRepository;

        public DashboardService(
            IDashboardRepository dashboardRepository,
            IDashboardPageRepository dashboardPageRepository,
            IDashboardPageModuleRepository dashboardPageModuleRepository)
        {
            _dashboardRepository = dashboardRepository;
            _dashboardPageRepository = dashboardPageRepository;
            _dashboardPageModuleRepository = dashboardPageModuleRepository;
        }

        public async Task<DashboardDto> GetDashboardDtoAsync(Guid moduleId, bool isSuperUser, string[] userRoles)
        {
            var dashboard = await _dashboardRepository.GetByModuleIdAsync(moduleId);
            var pages = await _dashboardPageRepository.GetsAsync(dashboard.Id, "ViewOrder");

            return HybridMapper.Map<DashboardInfo, DashboardDto>(
                source: dashboard,
                configAction: (src, dest) =>
                {
                    dest.Pages = BuildPageTree(pages, isSuperUser, userRoles);
                }
            );
        }

        public string[] GetDashboardAuthorization(Guid moduleId)
        {
            var roles = _dashboardRepository.GetAuthorizationViewDashboard(moduleId);
            return roles?.Split(',') ?? Array.Empty<string>();
        }

        public async Task<IEnumerable<DashboardPageDto>> GetDashboardPagesDtoAsync(Guid moduleId, bool isSuperUser, string[] userRoles, string currentPage)
        {
            var dashboardId = await _dashboardRepository.GetIdByModuleIdAsync(moduleId);
            var pages = await _dashboardPageRepository.GetsAsync(dashboardId, "ViewOrder");

            return BuildPageTree(pages, isSuperUser, userRoles, currentPage);
        }

        public DashboardPageModuleDto GetDashboardPageModule(Guid dashboardModuleId, string pageName)
        {
            var module = _dashboardPageModuleRepository.Get(dashboardModuleId, pageName);
            return HybridMapper.Map<DashboardPageModuleSpResult, DashboardPageModuleDto>(module);
        }

        private IEnumerable<DashboardPageDto> BuildPageTree(
            IEnumerable<DashboardPageInfo> pages, bool isSuperUser, string[] userRoles, string currentPage = "")
        {
            var filteredPages = pages.Where(p => isSuperUser ||
                                        p.InheritPermissionFromDashboard ||
                                        string.IsNullOrEmpty(p.AuthorizationViewPage) ||
                                        p.AuthorizationViewPage.Split(',').Any(r => userRoles.Contains(r)))
                                  .OrderBy(p => p.ViewOrder)
                                  .ToList();

            var pageLookup = filteredPages.ToLookup(p => p.ParentId);
            var openIds = GetAncestorIds(filteredPages, currentPage);

            return PopulateDashboardPages(null, pageLookup, openIds, currentPage);
        }

        private static HashSet<Guid> GetAncestorIds(List<DashboardPageInfo> pages, string currentPage)
        {
            var openIds = new HashSet<Guid>();
            if (string.IsNullOrEmpty(currentPage))
                return openIds;

            var byId = pages.ToDictionary(p => p.Id);
            var current = pages.FirstOrDefault(p => p.PageName == currentPage);
            while (current?.ParentId != null && byId.TryGetValue(current.ParentId.Value, out var parent))
            {
                openIds.Add(parent.Id);
                current = parent;
            }

            return openIds;
        }

        private IEnumerable<DashboardPageDto> PopulateDashboardPages(
            Guid? parentId, ILookup<Guid?, DashboardPageInfo> lookup, HashSet<Guid> openIds, string currentPage = "")
        {
            var siblings = lookup[parentId].ToList();
            if (siblings.Count == 0)
                return Enumerable.Empty<DashboardPageDto>();

            var lastIndex = siblings.Count - 1;

            var result = HybridMapper.MapCollection<DashboardPageInfo, DashboardPageDto>(
                sources: siblings,
                configAction: (src, dest) =>
                {
                    var children = lookup[src.Id];
                    dest.IsCurrent = dest.PageType == DashboardPageType.Standard && src.PageName == currentPage;
                    dest.IsParent = children.Any();
                    dest.IsChild = parentId != null;
                    dest.IsOpen = openIds.Contains(src.Id);
                    dest.Pages = dest.IsParent
                        ? PopulateDashboardPages(src.Id, lookup, openIds, currentPage)
                        : Enumerable.Empty<DashboardPageDto>();
                }
            ).ToList();

            for (var i = 0; i < result.Count; i++)
            {
                result[i].IsFirst = i == 0;
                result[i].IsLast = i == lastIndex;
            }

            return result;
        }
    }
}
