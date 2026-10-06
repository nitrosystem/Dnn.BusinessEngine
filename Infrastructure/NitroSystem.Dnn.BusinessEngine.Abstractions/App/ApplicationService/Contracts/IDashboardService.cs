using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Web.Dto;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Contracts
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardDtoAsync(Guid moduleId, bool isSuperUser, string[] userRoles);
        string[] GetDashboardAuthorization(Guid moduleId);
        Task<IEnumerable<DashboardPageDto>> GetDashboardPagesDtoAsync(Guid moduleId, bool isSuperUser, string[] userRoles, string currentPage);
        DashboardPageModuleDto GetDashboardPageModule(Guid dashboardModuleId, string pageName);
    }
}
