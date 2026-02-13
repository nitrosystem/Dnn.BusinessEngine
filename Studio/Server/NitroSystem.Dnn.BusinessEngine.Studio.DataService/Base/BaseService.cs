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
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Procedures;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Contracts;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Export;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Import;
using System.Text.RegularExpressions;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataService.Base
{
    public class BaseService : IBaseService, IExportable,IImportable
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

        public async Task<IEnumerable<GroupViewModel>> GetGroupsViewModelAsync(Guid scenarioId, string groupDomain)
        {
            var groups = await _repository.GetItemsByColumnsAsync<GroupInfo>(
                new string[2] { "ScenarioId", "GroupDomain" },
                new
                {
                    ScenarioId = scenarioId,
                    GroupDomain = groupDomain
                }
            );

            return HybridMapper.MapCollection<GroupInfo, GroupViewModel>(groups);
        }

        public async Task<IEnumerable<ExplorerItemViewModel>> GetGroupItemsAsync(Guid groupId, string groupType)
        {

            var items = await _repository.ExecuteStoredProcedureAsListAsync<GroupItemResult>(
                "dbo.BusinessEngine_Studio_GetGroupItems", "",
                new
                {
                    GroupId = groupId,
                    GroupType = groupType
                });

            return HybridMapper.MapCollection<GroupItemResult, ExplorerItemViewModel>(items);
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

        #region Import Export

        public async Task<ExportResponse> ExportAsync(ExportContext context)
        {
            switch (context.Scope)
            {
                case ImportExportScope.ScenarioFullComponents:
                    var items = await GetScenarioAndGroupsAsync(context.Get<Guid>("ScenarioId"));

                    return new ExportResponse()
                    {
                        Result = items,
                        IsSuccess = true
                    };
                default:
                    return null;
            }
        }

        public async Task<ImportResponse> ImportAsync(string json, ImportContext context)
        {
            switch (context.Scope)
            {
                case ImportExportScope.ScenarioFullComponents:
                    var items = JsonConvert.DeserializeObject<List<object>>(json);
                    var scenario = JsonConvert.DeserializeObject<ScenarioInfo>(items[0].ToString());
                    var groups = JsonConvert.DeserializeObject<IEnumerable<GroupInfo>>(items[1].ToString());

                    await SaveScenarioAndGroupsAsync(scenario, groups);

                    context.Set<string>("ScenarioName", scenario.ScenarioName);
                    break;
            }

            return new ImportResponse()
            {
                IsSuccess = true
            };
        }

        private async Task<object> GetScenarioAndGroupsAsync(Guid scenarioId)
        {
            var scenario = await _repository.GetAsync<ScenarioInfo>(scenarioId);
            var groups = await _repository.GetByScopeAsync<GroupInfo>(scenarioId);

            return new List<object>() { scenario, groups };
        }

        private async Task SaveScenarioAndGroupsAsync(ScenarioInfo scenario,IEnumerable<GroupInfo> groups)
        {
            await _repository.AddAsync<ScenarioInfo>(scenario);
            await _repository.BulkInsertAsync<GroupInfo>(groups);
        }

        #endregion
    }
}
