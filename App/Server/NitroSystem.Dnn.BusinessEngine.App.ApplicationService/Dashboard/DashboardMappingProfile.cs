using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Dashboard;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Dto;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;

namespace NitroSystem.Dnn.BusinessEngine.App.ApplicationService.Module
{
    public static class DashboardMappingProfile
    {
        public static void Register()
        {
            HybridMapper.AfterMap<DashboardInfo, DashboardDto>(
                (src, dest) => dest.AuthorizationViewDashboard = src.AuthorizationViewDashboard?.Split(','));

            HybridMapper.AfterMap<DashboardPageInfo, DashboardPageDto>(
                (src, dest) => dest.PageType = (DashboardPageType)src.PageType);
        }
    }
}
