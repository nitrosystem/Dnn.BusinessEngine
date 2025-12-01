using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;

namespace NitroSystem.Dnn.BusinessEngine.App.DataService.Module
{
    public static class DashboardMappingProfile
    {
        public static void Register()
        {
            HybridMapper.BeforeMap<DashboardInfo, DashboardDto>(
                (src, dest) => dest.AuthorizationViewDashboard = src.AuthorizationViewDashboard?.Split(','));

            HybridMapper.BeforeMap<DashboardPageInfo, DashboardPageDto>(
                (src, dest) => dest.PageType = (DashboardPageType)src.PageType);
        }
    }
}
