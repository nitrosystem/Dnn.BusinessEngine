using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using DotNetNuke.Security.Roles;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Core.Security;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Views;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Base;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Data.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Export;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Attributes;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataService.Base
{
    public class BaseService : ExportableBase, IBaseService, IExportable
    {
        private readonly ICacheService _cacheService;
        private readonly IRepositoryBase _repository;

        public BaseService(ICacheService cacheService, IRepositoryBase repository)
        {
            _cacheService = cacheService;
            _repository = repository;
        }

        #region General Service

        public async Task<string[]> GetPortalRolesAsync(int portalId)
        {
            string cacheKey = "bPortalRoles";
            string[] result = _cacheService.Get<string[]>(cacheKey);

            if (result == null)
            {
                var roles = await Task.Run(() => RoleController.Instance.GetRoles(portalId)
                    .Cast<RoleInfo>()
                    .Select(r => r.RoleName)
                    .ToArray()
                );

                var allUsers = new string[] { "All Users" };
                result = allUsers.Concat(roles).ToArray();

                _cacheService.Set(cacheKey, result);
            }

            return result;
        }

        #endregion

        #region Scenario

        public async Task<IEnumerable<ScenarioViewModel>> GetScenariosViewModelAsync()
        {
            var scenarios = await _repository.GetAllAsync<ScenarioInfo>();

            return HybridMapper.MapCollection<ScenarioInfo, ScenarioViewModel>(scenarios);
        }

        [Exportable]
        public async Task<ScenarioViewModel> GetScenarioViewModelAsync(Guid scenarioId)
        {
            var scenario = await _repository.GetAsync<ScenarioInfo>(scenarioId);

            return HybridMapper.Map<ScenarioInfo, ScenarioViewModel>(scenario);
        }

        public async Task<ScenarioViewModel> GetScenarioByNameViewModelAsync(string scenarioName)
        {
            var scenario = await _repository.GetByColumnAsync<ScenarioInfo>("ScenarioName", scenarioName);

            return HybridMapper.Map<ScenarioInfo, ScenarioViewModel>(scenario);
        }

        public async Task<string> GetScenarioNameAsync(Guid scenarioId)
        {
            return await _repository.GetColumnValueAsync<ScenarioInfo, string>(scenarioId, "ScenarioName");
        }

        public async Task<Guid> SaveScenarioAsync(ScenarioViewModel scenario, bool isNew)
        {
            var objScenarioInfo = HybridMapper.Map<ScenarioViewModel, ScenarioInfo>(scenario);

            if (isNew)
                objScenarioInfo.Id = await _repository.AddAsync(objScenarioInfo);
            else
            {
                var isUpdated = await _repository.UpdateAsync(objScenarioInfo);
                if (!isUpdated) ErrorHandling.ThrowUpdateFailedException(objScenarioInfo);
            }

            return objScenarioInfo.Id;
        }

        public async Task<bool> DeleteScenarioAsync(Guid scenarioId)
        {
            return await _repository.DeleteAsync<ScenarioInfo>(scenarioId);
        }

        #endregion

        #region Group

        public async Task<IEnumerable<GroupViewModel>> GetGroupsViewModelAsync(Guid scenarioId, string groupType = null)
        {
            var groups = await _repository.GetByScopeAsync<GroupInfo>(scenarioId);
            if (!string.IsNullOrEmpty(groupType)) groups = groups.Where(g => g.GroupType == groupType);

            return HybridMapper.MapCollection<GroupInfo, GroupViewModel>(groups);
        }

        public async Task<Guid> SaveGroupAsync(GroupViewModel group, bool isNew)
        {
            var objGroupInfo = HybridMapper.Map<GroupViewModel, GroupInfo>(group);

            if (isNew)
                objGroupInfo.Id = await _repository.AddAsync(objGroupInfo);
            else
            {
                var isUpdated = await _repository.UpdateAsync(objGroupInfo);
                if (!isUpdated) ErrorHandling.ThrowUpdateFailedException(objGroupInfo);
            }

            return objGroupInfo.Id;
        }

        public async Task<bool> DeleteGroupAsync(Guid groupId)
        {
            return await _repository.DeleteAsync<GroupInfo>(groupId);
        }

        #endregion

        #region Explorer Items

        public async Task<IEnumerable<ExplorerItemViewModel>> GetExplorerItemsViewModelAsync(Guid scenarioId)
        {
            var items = await _repository.GetByScopeAsync<ExplorerItemView>(scenarioId, "ViewOrder");

            return HybridMapper.MapCollection<ExplorerItemView, ExplorerItemViewModel>(items);
        }

        #endregion

        #region Library & Resources

        public async Task<IEnumerable<LibraryListItem>> GetLibrariesListItemAsync()
        {
            var libraries = await _repository.GetAllAsync<LibraryInfo>();
            var resources = await _repository.GetAllAsync<LibraryResourceInfo>();

            return HybridMapper.MapWithChildren<LibraryInfo, LibraryListItem,
                                                LibraryResourceInfo, LibraryResourceListItem>(
                libraries,
                resources,
                parentKeySelector: p => p.Id,
                childKeySelector: c => c.LibraryId,
                assignChildren: (parent, childs) => parent.Resources = childs
            );
        }

        #endregion

        public async Task<T> Export<T>(string methodName, params object[] args) where T : class
        {
            var data = await base.Export<object>(this,typeof(BaseService), methodName, args);
            return data as T;

        }
    }
}
