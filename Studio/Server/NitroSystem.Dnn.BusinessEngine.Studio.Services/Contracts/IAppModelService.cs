using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetNuke.Entities.Portals;
using NitroSystem.Dnn.BusinessEngine.Studio.DataServices.ViewModels.AppModel;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataServices.Contracts
{
    public interface IAppModelService
    {
        Task<AppModelViewModel> GetAppModelAsync(Guid appModelId);
        Task<IEnumerable<AppModelViewModel>> GetAppModelsAsync(Guid scenarioId, string sortBy = "ViewOrder");
        Task<(IEnumerable<AppModelViewModel> Items, int? TotalCount)> GetAppModelsAsync(Guid scenarioId, int pageIndex, int pageSize, string searchText, string sortBy);
        Task<Guid> SaveAppModelAsync(AppModelViewModel appModel, bool isNew, PortalSettings portalSettings);
        Task<bool> UpdateGroupColumnAsync(Guid appModelId, Guid? groupId);
        Task<bool> DeleteAppModelAsync(Guid appModelId);
    }
}
