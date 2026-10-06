using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.AppModel;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts
{
    public interface IAppModelService
    {
        Task<(IEnumerable<AppModelViewModel> Items, int? TotalCount)> GetAppModelsViewModelAsync(
            Guid scenarioId,
            int pageIndex,
            int pageSize,
            string searchText,
            AppModelType? modelType,
            string sortBy);
        Task<IEnumerable<AppModelListItem>> GetAppModelsListItemAsync(Guid scenarioId, string sortBy = "ViewOrder");
        Task<AppModelViewModel> GetAppModelViewModelAsync(Guid appModelId);

        Task<Guid> CreateAppModelAsync(AppModelViewModel appModel, string relativePath, string basePath);
        Task UpdateAppMopdelGroupAsync(Guid? groupId, Guid appModelId);

        Task<bool> DeleteAppModelAsync(Guid appModelId);
    }
}
