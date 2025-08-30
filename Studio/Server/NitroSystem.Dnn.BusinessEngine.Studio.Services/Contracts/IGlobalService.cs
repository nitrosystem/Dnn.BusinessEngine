using DotNetNuke.Data;
using DotNetNuke.Security.Roles;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Cashing;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Dto;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Mapping;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts
{
    public interface IGlobalService
    {
        #region General Service

        Task<string[]> GetPortalRolesAsync(int portalId);
        
        #endregion

        #region Scenario

        Task<IEnumerable<ScenarioViewModel>> GetScenariosViewModelAsync();
        
        Task<ScenarioViewModel> GetScenarioViewModelAsync(Guid scenarioId);

        Task<ScenarioViewModel> GetScenarioByNameViewModelAsync(string name);

        Task<string> GetScenarioNameAsync(Guid scenarioId);

        Task<Guid> SaveScenarioAsync(ScenarioViewModel scenario, bool isNew);

        void DeleteScenarioAndChilds(Guid scenarioId);

        #endregion

        #region Group

        Task<IEnumerable<GroupViewModel>> GetGroupsViewModelAsync(Guid scenarioId, string groupType = null);

        Task<Guid> SaveGroupAsync(GroupViewModel group, bool isNew);

        Task<bool> DeleteGroupAsync(Guid groupId);

        #endregion

        #region Explorer Items

        Task<IEnumerable<ExplorerItemViewModel>> GetExplorerItemsViewModelAsync(Guid scenarioId);

        #endregion

        #region Library & Resources

        Task<IEnumerable<LibraryDto>> GetLibrariesLiteDtoAsync();

        #endregion

        #region Studio Library

        Task<IEnumerable<StudioLibraryViewModel>> GetStudioLibrariesViewModelAsync();

        #endregion
    }
}
