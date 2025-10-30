using System;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Dto;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Contracts
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardDtoAsync(Guid moduleId);
    }
}
