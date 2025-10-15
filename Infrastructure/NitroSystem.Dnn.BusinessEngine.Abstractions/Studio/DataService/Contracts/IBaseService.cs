using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Base;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts
{
    public interface IBaseService
    {
        Task<string[]> GetPortalRolesAsync(int portalId);

        Task<IEnumerable<ScenarioViewModel>> GetScenariosViewModelAsync();
        Task<ScenarioViewModel> GetScenarioViewModelAsync(Guid scenarioId);
        Task<ScenarioViewModel> GetScenarioByNameViewModelAsync(string name);
        Task<string> GetScenarioNameAsync(Guid scenarioId);
        Task<Guid> SaveScenarioAsync(ScenarioViewModel scenario, bool isNew);
        Task<bool> DeleteScenarioAsync(Guid id);

        Task<IEnumerable<GroupViewModel>> GetGroupsViewModelAsync(Guid scenarioId, string groupType = null);
        Task<Guid> SaveGroupAsync(GroupViewModel group, bool isNew);
        Task<bool> DeleteGroupAsync(Guid groupId);

        Task<IEnumerable<ExplorerItemViewModel>> GetExplorerItemsViewModelAsync(Guid scenarioId);

        Task<IEnumerable<LibraryListItem>> GetLibrariesListItemAsync();
    }
}
