using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Base;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts
{
    public interface IBaseService
    {
        #region Common
        
        Task<(IEnumerable<string> Tables, IEnumerable<string> Views)> GetDatabaseObjectsAsync();
        Task<IEnumerable<DbTableColumnInfo>> GetDatabaseObjectColumnsAsync(string objectName);

        #endregion

        #region Scenario

        Task<IEnumerable<ScenarioViewModel>> GetScenariosViewModelAsync();
        Task<ScenarioViewModel> GetScenarioViewModelAsync(Guid scenarioId);
        Task<ScenarioViewModel> GetScenarioViewModelByNameAsync(string scenarioName);
        Task<string> GetScenarioNameAsync(Guid scenarioId);
        
        Task<Guid> CreateScenarioAsync(ScenarioViewModel scenario);
        
        Task<bool> DeleteScenarioAsync(Guid id);

        #endregion

        #region Group
        
        Task<IEnumerable<GroupViewModel>> GetGroupsViewModelAsync(Guid scenarioId, string groupDomain);

        Task<Guid> CreateGroupAsync(GroupViewModel group);

        Task<bool> DeleteGroupAsync(Guid groupId);

        #endregion

        #region Explorer Items 

        Task<IEnumerable<ExplorerItemViewModel>> GetExplorerItemsViewModelAsync(Guid scenarioId);
        Task<IEnumerable<LibraryListItem>> GetLibrariesListItemAsync(bool includeSystemLibraries = false);

        #endregion
    }
}
