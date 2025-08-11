using NitroSystem.Dnn.BusinessEngine.Studio.Services.Mapping;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.Cashing;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Roles;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Dto;
using NitroSystem.Dnn.BusinessEngine.Common.Reflection;
using NitroSystem.Dnn.BusinessEngine.Core.Mapper;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Services
{
    public class GlobalService : IGlobalService
    {
        private readonly ICacheService _cacheService;
        private readonly IRepositoryBase _repository;

        private static Dictionary<Guid, ScenarioViewModel> _cachedScenarios;
        private static Dictionary<string, ScenarioViewModel> _cachedNameScenarios;

        public GlobalService(ICacheService cacheService, IRepositoryBase repository)
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
            var cacheAttr = AttributeCache.Instance.GetCache<ScenarioInfo>();

            var scenarios = await _repository.GetAllAsync<ScenarioInfo>();

            return BaseMapping<ScenarioInfo, ScenarioViewModel>.MapViewModels(scenarios);
        }

        public async Task<ScenarioViewModel> GetScenarioViewModelAsync(Guid scenarioId)
        {
            var scenarios = await GetScenariosViewModelAsync();
            _cachedScenarios = scenarios.ToDictionary(s => s.Id);

            return _cachedScenarios.TryGetValue(scenarioId, out var scenario) ? scenario : null;
        }

        public async Task<ScenarioViewModel> GetScenarioByNameViewModelAsync(string name)
        {
            var scenarios = await GetScenariosViewModelAsync();
            _cachedNameScenarios = scenarios.ToDictionary(s => s.ScenarioName);

            return _cachedNameScenarios.TryGetValue(name, out var scenario) ? scenario : null;
        }

        public async Task<string> GetScenarioNameAsync(Guid scenarioId)
        {
            return await _repository.GetColumnValueAsync<ScenarioInfo, string>(scenarioId, "ScenarioName");
        }

        public async Task<Guid> SaveScenarioAsync(ScenarioViewModel scenario, bool isNew)
        {
            var objScenarioInfo = BaseMapping<ScenarioInfo, ScenarioViewModel>.MapEntity(scenario);

            if (isNew)
                objScenarioInfo.Id = await _repository.AddAsync<ScenarioInfo>(objScenarioInfo);
            else
            {
                var isUpdated = await _repository.UpdateAsync<ScenarioInfo>(objScenarioInfo);
                if (!isUpdated) ErrorService.ThrowUpdateFailedException(objScenarioInfo);
            }

            var cacheAttr = AttributeCache.Instance.GetCache<ScenarioInfo>();

            return objScenarioInfo.Id;
        }

        public void DeleteScenarioAndChilds(Guid scenarioId)
        {
            //var relationships = GlobalRepository.Instance.GetRelationships();
            //var items = DbUtil.GetOrderedTables(relationships);
            //var mustBeDeleted = relationships.Where(r => r.ParentTable == BaseEntity.Scenario.TableName).Select(r => r.ChildTable);
            //var finalItems = items.Where(i => i == BaseEntity.Scenario.TableName || mustBeDeleted.Contains(i)).Reverse();

            //_repository.DeleteEntitiesRow<Guid>(finalItems, "ScenarioId", scenarioId);
            //_unitOfWork.Commit();
        }

        #endregion

        #region Group

        public async Task<IEnumerable<GroupViewModel>> GetGroupsViewModelAsync(Guid scenarioId, string groupType = null)
        {
            var groups = await _repository.GetByScopeAsync<GroupInfo>(scenarioId);
            if (!string.IsNullOrEmpty(groupType)) groups = groups.Where(g => g.GroupType == groupType);

            return BaseMapping<GroupInfo, GroupViewModel>.MapViewModels(groups);
        }

        public async Task<Guid> SaveGroupAsync(GroupViewModel group, bool isNew)
        {
            var objGroupInfo = BaseMapping<GroupInfo, GroupViewModel>.MapEntity(group);

            if (isNew)
                objGroupInfo.Id = await _repository.AddAsync<GroupInfo>(objGroupInfo);
            else
            {
                var isUpdated = await _repository.UpdateAsync<GroupInfo>(objGroupInfo);
                if (!isUpdated) ErrorService.ThrowUpdateFailedException(objGroupInfo);
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

            return BaseMapping<ExplorerItemView, ExplorerItemViewModel>.MapViewModels(items);
        }

        #endregion

        #region Library & Resources

        public async Task<IEnumerable<LibraryDto>> GetLibrariesLiteDtoAsync()
        {
            var libraries = await _repository.GetAllAsync<LibraryInfo>();
            var resources = await _repository.GetAllAsync<LibraryResourceInfo>();

            return libraries.Select(library =>
                HybridMapper.MapWithConfig<LibraryInfo, LibraryDto>(library,
                (src, dest) =>
                {
                    dest.Resources = resources.Where(r => r.LibraryId == library.Id).Select(resource =>
                        HybridMapper.Map<LibraryResourceInfo, LibraryResourceDto>(resource)
                    );
                })
            );
        }

        public async Task<IEnumerable<LibraryResourceViewModel>> GetLibraryResourcesViewModelAsync(Guid libraryId)
        {
            var resources = await _repository.GetByScopeAsync<LibraryResourceInfo>(libraryId);

            return resources.Select(resource =>
            {
                var result = new LibraryResourceViewModel();
                PropertyCopier<LibraryResourceInfo, LibraryResourceViewModel>.Copy(resource, result);
                return result;
            });
        }

        #endregion

        #region Studio Library

        public async Task<IEnumerable<StudioLibraryViewModel>> GetStudioLibrariesViewModelAsync()
        {
            var cacheAttr = AttributeCache.Instance.GetCache<StudioLibraryInfo>();
            var libraries = await _repository.GetAllAsync<StudioLibraryInfo>();

            return BaseMapping<StudioLibraryInfo, StudioLibraryViewModel>.MapViewModels(libraries);
        }

        #endregion
    }
}
