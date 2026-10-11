using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.DefinedList;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.DefinedList;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.DefinedList;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Export;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Import;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;

namespace NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.DefinedList
{
    public class DefinedListService : IDefinedListService, IExportable, IImportable
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDefinedListRepository _definedListRepository;
        private readonly IDefinedListItemRepository _definedListItemRepository;
        private readonly IModuleFieldDataSourceRepository _moduleFieldDataSourceRepository;

        public DefinedListService(IUnitOfWork unitOfWork, IDefinedListRepository definedListRepository, IDefinedListItemRepository definedListItemRepository, IModuleFieldDataSourceRepository moduleFieldDataSourceRepository)
        {
            _unitOfWork = unitOfWork;
            _definedListRepository = definedListRepository;
            _definedListItemRepository = definedListItemRepository;
            _moduleFieldDataSourceRepository = moduleFieldDataSourceRepository;
        }

        public async Task<IEnumerable<DefinedListViewModel>> GetDefinedListsViewModelAsync(Guid scenarioId)
        {
            var definedLists = await _definedListRepository.GetsAsync(scenarioId, "ListName");
            var definedListsItems = await _definedListItemRepository.GetsByAncestorAsync(scenarioId);

            return HybridMapper.MapWithChildren<DefinedListInfo, DefinedListViewModel, DefinedListItemInfo, DefinedListItemViewModel>(
              parents: definedLists,
              children: definedListsItems,
              parentKeySelector: p => p.Id,
              childKeySelector: c => c.ListId,
              assignChildren: (parent, childs) => parent.Items = childs
            );
        }

        public async Task<DefinedListViewModel> GetDefinedListViewModelByListNameAsync(string listName, string sortBy = "ViewOrder")
        {
            var objDefinedListInfo = await _definedListRepository.GetAsync(listName);
            if (objDefinedListInfo != null)
            {
                var items = await _definedListItemRepository.GetsAsync(objDefinedListInfo.Id, sortBy);
                return HybridMapper.MapWithChildren<DefinedListInfo, DefinedListViewModel, DefinedListItemInfo, DefinedListItemViewModel>(
                    source: objDefinedListInfo,
                    children: items,
                    assignChildren: (parent, childs) => parent.Items = childs);
            }

            return null;
        }

        public async Task<Guid> CreateDefinedListAsync(DefinedListViewModel definedList)
        {
            var objDefinedListInfo = HybridMapper.Map<DefinedListViewModel, DefinedListInfo>(definedList);
            var listItems = HybridMapper.MapCollection<DefinedListItemViewModel, DefinedListItemInfo>(definedList.Items);

            _unitOfWork.BeginTransaction();

            try
            {
                if (objDefinedListInfo.Id == Guid.Empty)
                {
                    objDefinedListInfo.Id = await _definedListRepository.AddAsync(objDefinedListInfo);
                }
                else
                {
                    await _definedListRepository.UpdateAsync(objDefinedListInfo);

                    //Delete old DefinedListItems
                    await _definedListItemRepository.DeletesAsync(definedList.Id);
                }

                //Add new DefinedListItems
                await _definedListItemRepository.BulkInsertAsync(listItems.Select(i => { i.ListId = objDefinedListInfo.Id; return i; }).ToList());

                _unitOfWork.Commit();

                return objDefinedListInfo.Id;
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                throw ex;
            }
        }

        #region Import Export

        public async Task<ExportResponse> ExportAsync(ExportContext context)
        {
            if (context.Scope == ImportExportScope.ScenarioFullComponents)
            {
                var items = await GetScenarioDefinedListsAsync(context.Get<Guid>("ScenarioId"));

                return new ExportResponse()
                {
                    Result = items,
                    IsSuccess = true
                };
            }
            else if (context.Scope == ImportExportScope.Module)
            {
                var moduleId = context.Get<Guid>("ModuleId");
                var items = await GetDefinedListsAsync(moduleId);

                return new ExportResponse()
                {
                    Result = items,
                    IsSuccess = true
                };
            }

            return null;
        }

        public async Task<ImportResponse> ImportAsync(string json, ImportContext context)
        {
            var items = JsonConvert.DeserializeObject<List<object>>(json);
            var definedLists = JsonConvert.DeserializeObject<IReadOnlyList<DefinedListInfo>>(items[0].ToString());
            var definedListsItems = JsonConvert.DeserializeObject<IReadOnlyList<DefinedListItemInfo>>(items[1].ToString());

            if (context.Scope == ImportExportScope.ScenarioFullComponents)
            {
                await BulkInsertListsAndItemsAsync(definedLists, definedListsItems);
            }
            else if (context.Scope == ImportExportScope.Module)
            {
                var moduleId = (Guid)context.DataTrack["ModuleId"];
                var scenarioId = (Guid)context.DataTrack["ScenarioId"];
                var oldScenarioId = (Guid)context.DataTrack["OldScenarioId"];

                if (oldScenarioId != scenarioId)
                    await BulkInsertListsAndItemsAsync(definedLists, definedListsItems);
            }

            return new ImportResponse()
            {
                IsSuccess = true
            };
        }

        private async Task<object> GetScenarioDefinedListsAsync(Guid scenarioId)
        {
            var definedLists = await _definedListRepository.GetsAsync(scenarioId);
            var definedListsItems = new List<DefinedListItemInfo>();

            foreach (var definedList in definedLists)
            {
                definedListsItems.AddRange(await _definedListItemRepository.GetsAsync(definedList.Id));
            }

            return new List<object>() { definedLists, definedListsItems };
        }

        private async Task<object> GetDefinedListsAsync(Guid moduleId)
        {
            var definedLists = new List<DefinedListInfo>();
            var definedListsItems = new List<DefinedListItemInfo>();

            var listIds = await _moduleFieldDataSourceRepository.GetModuleListIdsAsync(moduleId);
            foreach (var listId in listIds)
            {
                var definedList = await _definedListRepository.GetAsync(listId);

                definedLists.Add(definedList);
                definedListsItems.AddRange(await _definedListItemRepository.GetsAsync(definedList.Id));
            }

            return new List<object>() { definedLists, definedListsItems };
        }

        private async Task BulkInsertListsAndItemsAsync(IReadOnlyList<DefinedListInfo> definedLists, IReadOnlyList<DefinedListItemInfo> definedListsItems)
        {
            await _definedListRepository.BulkInsertAsync(definedLists);
            await _definedListItemRepository.BulkInsertAsync(definedListsItems);
        }

        #endregion
    }
}
