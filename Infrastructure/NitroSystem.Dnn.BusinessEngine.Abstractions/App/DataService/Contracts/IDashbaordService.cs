using DotNetNuke.Data;
using NitroSystem.Dnn.BusinessEngine.Common.Reflection;
using NitroSystem.Dnn.BusinessEngine.Core.Cashing;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Core.Providers;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;
using NitroSystem.Dnn.BusinessEngine.App.Services.Dto;
using NitroSystem.Dnn.BusinessEngine.App.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using NitroSystem.Dnn.BusinessEngine.App.Services.ViewModels.Dashboard.Skin;

namespace NitroSystem.Dnn.BusinessEngine.App.Services.Contracts
{
    public interface IDashbaordService
    {
        Task<Guid?> GetDashboardIdAsync(Guid moduleId);

        Task<DashboardDto> GetDashboardDtoAsync(Guid moduleId);

        Task<IEnumerable<DashboardPageDto>> GetDashboardPagesViewModelAsync(Guid moduleId);

        Task<DashboardPageDto> GetDashboardPageDtoAsync(Guid pageId);

        Task<IEnumerable<DashboardPageLiteDto>> GetDashboardPagesLiteDtoAsync(Guid dashboardModuleId);

        Task<(Guid, Guid)> SaveDashboardBasicInfoAsync(DashboardDto dashboard);

        Task<(Guid?, Guid?, Guid?)> SaveDashboardPageAsync(DashboardPageDto page);

        Task<(DashboardAppearanceDto, IEnumerable<DashboardSkinDto>, IEnumerable<DashboardTemplateDto>)> GetDashboardAppearanceAsync(
            Guid moduleId, HttpContext context);

        Task<IEnumerable<DashboardTemplateDto>> GetDashboardTemplatesDtoAsync(
            DashboardType dashboardType, string skinName, string skinPath, HttpContext context);
        
        Task<IEnumerable<ModuleTemplateViewModel>> GetModuleTemplates(Guid dashboardModuleId, ModuleType moduleType, HttpContext context);

        Task SaveDashboardAppearanceAsync(DashboardAppearanceDto dashboard, HttpContext context);

        Task<IEnumerable<DashboardPageModuleDto>> GetDashboardPagesModule(Guid moduleId);

        Task<IEnumerable<ModuleCustomLibraryViewModel>> GetDashboardLibraries(Guid moduleId);

        Task<IEnumerable<ModuleCustomResourceViewModel>> GetDashboardResources(Guid moduleId);

        Task<Guid> SaveCustomLibrary(ModuleCustomLibraryDto library);

        Task<Guid> SaveCustomResource(ModuleCustomResourceDto resource);

        Task<bool> DeleteDashboardAsync(Guid id);

        Task<bool> DeleteLibraryAsync(Guid id);

        Task<bool> DeleteResourceAsync(Guid id);

    }
}
