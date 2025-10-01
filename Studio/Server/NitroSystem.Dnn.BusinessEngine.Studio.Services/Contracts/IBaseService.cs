using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ListItems;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Base;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts
{
    public interface IBaseService
    {
        Task<string[]> GetPortalRolesAsync(int portalId);

        Task<IEnumerable<ScenarioViewModel>> GetScenariosViewModelAsync();
        Task<ScenarioViewModel> GetScenarioViewModelAsync(Guid scenarioId);
        Task<ScenarioViewModel> GetScenarioByNameViewModelAsync(string name);
        Task<string> GetScenarioNameAsync(Guid scenarioId);
        Task<Guid> SaveScenarioAsync(ScenarioViewModel scenario, bool isNew);

        Task<IEnumerable<GroupViewModel>> GetGroupsViewModelAsync(Guid scenarioId, string groupType = null);
        Task<Guid> SaveGroupAsync(GroupViewModel group, bool isNew);
        Task<bool> DeleteGroupAsync(Guid groupId);

        Task<IEnumerable<ExplorerItemViewModel>> GetExplorerItemsViewModelAsync(Guid scenarioId);

        Task<IEnumerable<LibraryListItem>> GetLibrariesListItemAsync();
    }
}
