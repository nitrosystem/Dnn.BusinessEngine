using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Base;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Library;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Base;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Library;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Views.Base;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Base;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Export;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Import;

namespace NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.Base
{
    public class BaseService : IBaseService, IExportable, IImportable
    {
        private readonly ISql _sql;
        private readonly IScenarioRepository _scenarioRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly ILibraryRepository _libraryRepository;
        private readonly ILibraryResourceRepository _libraryResourceRepository;

        public BaseService(
            ISql sql,
            IScenarioRepository scenarioRepository,
            IGroupRepository groupRepository,
            ILibraryRepository libraryRepository,
            ILibraryResourceRepository libraryResourceRepository)
        {
            _sql = sql;
            _scenarioRepository = scenarioRepository;
            _groupRepository = groupRepository;
            _libraryRepository = libraryRepository;
            _libraryResourceRepository = libraryResourceRepository;

        }

        #region General Service

        public async Task<(IEnumerable<string> Tables, IEnumerable<string> Views)> GetDatabaseObjectsAsync()
        {
            var tables = await _sql.GetDatabaseObjectsAsync(0);
            var views = await _sql.GetDatabaseObjectsAsync(1);

            return (tables, views);
        }

        public async Task<IEnumerable<DbTableColumnInfo>> GetDatabaseObjectColumnsAsync(string objectName)
        {
            return await _sql.GetDatabaseObjectColumnsAsync(objectName);
        }

        #endregion

        #region Scenario

        public async Task<string> GetScenarioNameAsync(Guid scenarioId)
        {
            return await _scenarioRepository.GetScenarioNameAsync(scenarioId);
        }

        public async Task<ScenarioViewModel> GetScenarioViewModelAsync(Guid scenarioId)
        {
            var scenario = await _scenarioRepository.GetAsync(scenarioId);
            return HybridMapper.Map<ScenarioInfo, ScenarioViewModel>(scenario);
        }

        public async Task<ScenarioViewModel> GetScenarioViewModelByNameAsync(string scenarioName)
        {
            var scenario = await _scenarioRepository.GetAsync(scenarioName);
            return HybridMapper.Map<ScenarioInfo, ScenarioViewModel>(scenario);
        }

        public async Task<IEnumerable<ScenarioViewModel>> GetScenariosViewModelAsync()
        {
            var scenarios = await _scenarioRepository.GetsAsync();
            return HybridMapper.MapCollection<ScenarioInfo, ScenarioViewModel>(scenarios);
        }

        public async Task<Guid> CreateScenarioAsync(ScenarioViewModel scenario)
        {
            var objScenarioInfo = HybridMapper.Map<ScenarioViewModel, ScenarioInfo>(scenario);
            if (objScenarioInfo.Id == Guid.Empty)
                objScenarioInfo.Id = await _scenarioRepository.AddAsync(objScenarioInfo);
            else
                await _scenarioRepository.UpdateAsync(objScenarioInfo);

            return objScenarioInfo.Id;
        }

        public async Task<bool> DeleteScenarioAsync(Guid scenarioId)
        {
            return await _scenarioRepository.DeleteAsync(scenarioId);
        }

        #endregion

        #region Group

        public async Task<IEnumerable<GroupViewModel>> GetGroupsViewModelAsync(Guid scenarioId, string groupDomain)
        {
            var groups = await _groupRepository.GetsByDomainAsync(scenarioId, groupDomain);
            return HybridMapper.MapCollection<GroupInfo, GroupViewModel>(groups);
        }

        public async Task<Guid> CreateGroupAsync(GroupViewModel group)
        {
            var objGroupInfo = HybridMapper.Map<GroupViewModel, GroupInfo>(group);
            if (objGroupInfo.Id == Guid.Empty)
                objGroupInfo.Id = await _groupRepository.AddAsync(objGroupInfo);
            else
                await _groupRepository.UpdateAsync(objGroupInfo);

            return objGroupInfo.Id;
        }

        public async Task<bool> DeleteGroupAsync(Guid groupId)
        {
            return await _groupRepository.DeleteAsync(groupId);
        }

        #endregion

        #region Explorer Items

        public async Task<IEnumerable<ExplorerItemViewModel>> GetExplorerItemsViewModelAsync(Guid scenarioId)
        {
            var items = await _groupRepository.GetExplorerItemsViewModelAsync(scenarioId, "ViewOrder");
            return HybridMapper.MapCollection<ExplorerItemView, ExplorerItemViewModel>(items);
        }

        #endregion

        #region Library & Resources

        public async Task<IEnumerable<LibraryListItem>> GetLibrariesListItemAsync(bool includeSystemLibraries = false)
        {
            var libraries = await _libraryRepository.GetsAsync();
            var resources = await _libraryResourceRepository.GetsAsync();

            return HybridMapper.MapWithChildren<LibraryInfo, LibraryListItem,
                                                LibraryResourceInfo, LibraryResourceListItem>(
                libraries.Where(l => l.IsSystemLibrary == includeSystemLibraries),
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
                    var groups = JsonConvert.DeserializeObject<IReadOnlyList<GroupInfo>>(items[1].ToString());

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
            var scenario = await _scenarioRepository.GetAsync(scenarioId);
            var groups = await _groupRepository.GetsAsync(scenarioId);

            return new List<object>() { scenario, groups };
        }

        private async Task SaveScenarioAndGroupsAsync(ScenarioInfo scenario, IReadOnlyList<GroupInfo> groups)
        {
            await _scenarioRepository.AddAsync(scenario);
            await _groupRepository.BulkInsertAsync(groups);
        }

        #endregion
    }
}
