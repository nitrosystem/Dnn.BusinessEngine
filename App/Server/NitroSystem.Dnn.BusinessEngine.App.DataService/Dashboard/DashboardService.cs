using System;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Data.Contracts;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Dto;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Contracts;
using System.Collections.Generic;
using System.Linq;

namespace NitroSystem.Dnn.BusinessEngine.App.DataService.Module
{
    public class DashboardService : IDashboardService
    {
        private readonly IRepositoryBase _repository;

        public DashboardService(IRepositoryBase repository)
        {
            _repository = repository;
        }

        public async Task<DashboardDto> GetDashboardDtoAsync(Guid moduleId)
        {
            var dashboard = await _repository.GetByColumnAsync<DashboardInfo>("ModuleId", moduleId);
            var pages = await _repository.GetByScopeAsync<DashboardPageInfo>(dashboard.Id, "ViewOrder");

            return HybridMapper.Map<DashboardInfo, DashboardDto>(
                source: dashboard,
                configAction: (src, dest) =>
                {
                    dest.Pages = BuildPageTree(pages);
                }
            );
        }

        private IEnumerable<DashboardPageDto> BuildPageTree(IEnumerable<DashboardPageInfo> pages)
        {
            var pageLookup = pages.ToLookup(p => p.ParentId);
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
