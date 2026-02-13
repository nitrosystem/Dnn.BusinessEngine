using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Core.Security;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Base;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Data.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Contracts;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Export;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Import;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Views;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataService.DefinedList
{
    public class DefinedListService : IDefinedListService, IExportable, IImportable
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryBase _repository;
        private readonly IExecuteSqlCommand _sqlCommand;

        public DefinedListService(IUnitOfWork unitOfWork, IRepositoryBase repository, IExecuteSqlCommand sqlCommand)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
            _sqlCommand = sqlCommand;

        }

        public async Task<IEnumerable<DefinedListViewModel>> GetDefinedLists(Guid scenarioId)
        {
            var definedLists = await _repository.GetByScopeAsync<DefinedListInfo>(scenarioId, "ListName");
            var definedListsItems = await _repository.GetChildsByParentColumn<DefinedListInfo, DefinedListItemInfo>(
                "ScenarioId", "ListId", scenarioId);

            return HybridMapper.MapWithChildren<DefinedListInfo, DefinedListViewModel, DefinedListItemInfo, DefinedListItemViewModel>(
              parents: definedLists,
              children: definedListsItems,
              parentKeySelector: p => p.Id,
              childKeySelector: c => c.ListId,
              assignChildren: (parent, childs) => parent.Items = childs
            );
        }

        public async Task<DefinedListViewModel> GetDefinedListByListName(string listName, string sortBy = "ViewOrder")
        {
            var objDefinedListInfo = await _repository.GetByColumnAsync<DefinedListInfo>("ListName", listName);
            if (objDefinedListInfo != null)
            {
                var items = await _repository.GetByScopeAsync<DefinedListItemInfo>(objDefinedListInfo.Id, sortBy);

                return HybridMapper.MapWithChildren<DefinedListInfo, DefinedListViewModel, DefinedListItemInfo, DefinedListItemViewModel>(
                    source: objDefinedListInfo,
                    children: items,
                    assignChildren: (parent, childs) => parent.Items = childs);
            }

            return null;
        }

        public async Task<Guid> SaveDefinedList(DefinedListViewModel definedList, bool isNew)
        {
            IEnumerable<DefinedListItemInfo> items = Enumerable.Empty<DefinedListItemInfo>();

            var objDefinedListInfo = HybridMapper.MapWithChildren<DefinedListViewModel, DefinedListInfo, DefinedListItemViewModel, DefinedListItemInfo>(
                source: definedList,
                children: definedList.Items,
                assignChildren: (info, columns) => items = columns
            );

            _unitOfWork.BeginTransaction();

            try
            {
                if (isNew)
                    objDefinedListInfo.Id = await _repository.AddAsync(objDefinedListInfo);
                else
                {
                    var isUpdated = await _repository.UpdateAsync(objDefinedListInfo);
                    if (!isUpdated) ErrorHandling.ThrowUpdateFailedException(objDefinedListInfo);
                }

                if (definedList.Id != Guid.Empty) await _repository.DeleteAsync<DefinedListItemInfo>(definedList.Id);

                foreach (var objDefinedListItemInfo in items)
                {
                    objDefinedListItemInfo.ListId = objDefinedListInfo.Id;

                    if (objDefinedListItemInfo.Id == Guid.Empty)
                        await _repository.AddAsync(objDefinedListItemInfo);
                    else
                    {
                        var isUpdated = await _repository.UpdateAsync(objDefinedListItemInfo);
                        if (!isUpdated) ErrorHandling.ThrowUpdateFailedException(objDefinedListItemInfo);
                    }
                }

                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();

                throw ex;
            }

            return objDefinedListInfo.Id;
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
            var definedLists = JsonConvert.DeserializeObject<IEnumerable<DefinedListInfo>>(items[0].ToString());
            var definedListsItems = JsonConvert.DeserializeObject<IEnumerable<DefinedListItemInfo>>(items[1].ToString());

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
            var definedLists = await _repository.GetByScopeAsync<DefinedListInfo>(scenarioId);
            var definedListsItems = new List<DefinedListItemInfo>();

            foreach (var definedList in definedLists)
            {
                definedListsItems.AddRange(await _repository.GetByScopeAsync<DefinedListItemInfo>(definedList.Id));
            }

            return new List<object>() { definedLists, definedListsItems };
        }

        private async Task<object> GetDefinedListsAsync(Guid moduleId)
        {
            var definedLists = new List<DefinedListInfo>();
            var definedListsItems = new List<DefinedListItemInfo>();

            var listIds = await _sqlCommand.ExecuteSqlCommandTextAsync<Guid>(_unitOfWork,
                @"SELECT ListId FROM dbo.BusinessEngine_ModuleFieldDataSource WHERE ListId IS NOT NULL AND 
                       FieldId IN (SELECT Id BusinessEngine_ModuleFields WHERE ModuleId = @ModuleId)",
                new { ModuleId = moduleId });

            foreach (var listId in listIds)
            {
                var definedList = await _repository.GetAsync<DefinedListInfo>(listId);

                definedLists.Add(definedList);
                definedListsItems.AddRange(await _repository.GetByScopeAsync<DefinedListItemInfo>(definedList.Id));
            }

            return new List<object>() { definedLists, definedListsItems };
        }

        private async Task BulkInsertListsAndItemsAsync(IEnumerable<DefinedListInfo> definedLists, IEnumerable<DefinedListItemInfo> definedListsItems)
        {
            await _repository.BulkInsertAsync<DefinedListInfo>(definedLists);
            await _repository.BulkInsertAsync<DefinedListItemInfo>(definedListsItems);
        }

        #endregion
    }
}
