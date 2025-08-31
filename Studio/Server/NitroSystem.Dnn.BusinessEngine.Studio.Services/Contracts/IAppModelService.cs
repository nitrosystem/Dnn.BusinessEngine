using DotNetNuke.Data;
using DotNetNuke.Entities.Portals;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Mapping;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts
{
    public interface IAppModelService
    {
        #region View Model 

        Task<AppModelViewModel> GetAppModelAsync(Guid appModelId);

        Task<IEnumerable<AppModelViewModel>> GetAppModelsAsync(Guid scenarioId, string sortBy = "ViewOrder");

        Task<(IEnumerable<AppModelViewModel> Items, int? TotalCount)> GetAppModelsAsync(Guid scenarioId, int pageIndex, int pageSize, string searchText, string sortBy);

        Task<Guid> SaveAppModelAsync(AppModelViewModel appModel, bool isNew, PortalSettings portalSettings);

        Task<bool> UpdateGroupColumn(Guid appModelId, Guid? groupId);

        Task<bool> DeleteAppModelAsync(Guid appModelId);

        #endregion
    }
}
