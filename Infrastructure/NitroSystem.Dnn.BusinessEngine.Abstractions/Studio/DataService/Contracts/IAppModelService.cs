using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.AppModel;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts
{
    public interface IAppModelService
    {
        Task<(IEnumerable<AppModelViewModel> Items, int? TotalCount)> GetAppModelsAsync(
            Guid scenarioId,
            int pageIndex,
            int pageSize,
            string searchText,
            string sortBy);
        Task<IEnumerable<AppModelListItem>> GetAppModelsListItemAsync(Guid scenarioId, string sortBy = "ViewOrder");
        Task<AppModelViewModel> GetAppModelAsync(Guid appModelId);
        Task<Guid> SaveAppModelAsync(AppModelViewModel appModel, bool isNew);
        Task<bool> UpdateGroupColumnAsync(Guid appModelId, Guid? groupId);
        Task<bool> DeleteAppModelAsync(Guid appModelId);
    }
}
