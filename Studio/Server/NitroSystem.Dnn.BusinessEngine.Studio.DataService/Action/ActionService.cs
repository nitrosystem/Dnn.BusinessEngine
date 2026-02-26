using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Core.Security;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Views;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Action;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Data.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Import;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Export;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataService.Action
{
    public class ActionService : IActionService, IExportable, IImportable
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryBase _repository;

        public ActionService(IUnitOfWork unitOfWork, IRepositoryBase repository)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
        }

        public async Task<IEnumerable<ActionTypeListItem>> GetActionTypesListItemAsync(string sortBy = "GroupViewOrder")
        {
            var actionTypes = await _repository.GetAllAsync<ActionTypeView>(sortBy);
            return HybridMapper.MapCollection<ActionTypeView, ActionTypeListItem>(actionTypes);
        }

        public async Task<(IEnumerable<ActionViewModel> Items, int TotalCount)> GetActionsViewModelAsync(
            Guid moduleId, Guid? fieldId, int pageIndex, int pageSize, string searchText, string actionType, string sortBy)
        {
            var results = await _repository.ExecuteStoredProcedureMultipleAsync<int, ActionView, ActionParamInfo>(
                "dbo.BusinessEngine_Studio_GetActions", "BE_Actions_Studio_GetActions_",
                new
                {
                    ModuleId = moduleId,
                    FieldId = fieldId,
                    SearchText = searchText,
                    ActionType = actionType,
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    SortBy = sortBy
                });

            var totalCount = results.Item1?.First() ?? 0;
            var actions = results.Item2;
            var actionParams = results.Item3;

            var result = HybridMapper.MapWithChildren<ActionView, ActionViewModel, ActionParamInfo, ActionParamViewModel>(
               parents: actions,
               children: actionParams,
               parentKeySelector: p => p.Id,
               childKeySelector: c => c.ActionId,
               assignChildren: (parent, childs) => parent.Params = childs
            );

            return (result, totalCount);
        }

        public async Task<IEnumerable<ActionListItem>> GetActionsListItemAsync(Guid moduleId, string sortBy = "ViewOrder")
        {
            var actions = await _repository.GetByScopeAsync<ActionInfo>(moduleId, sortBy);
            return HybridMapper.MapCollection<ActionInfo, ActionListItem>(actions);
        }

        public async Task<ActionViewModel> GetActionViewModelAsync(Guid actionId)
        {
            var action = await _repository.GetAsync<ActionView>(actionId);
            var actionParams = await _repository.GetByScopeAsync<ActionParamInfo>(actionId);

            return HybridMapper.MapWithChildren<ActionView, ActionViewModel, ActionParamInfo, ActionParamViewModel>(
               source: action,
               children: actionParams,
               assignChildren: (parent, childs) => parent.Params = childs
           );
        }

        public async Task<Guid> SaveActionAsync(ActionViewModel action, bool isNew)
        {
            var objActionInfo = HybridMapper.Map<ActionViewModel, ActionInfo>(action,
                (src, dest) =>
                {
                    if (src.Event != "OnActionCompleted" && dest.ParentId.HasValue) dest.ParentId = null;
                });
            var actionParams = HybridMapper.MapCollection<ActionParamViewModel, ActionParamInfo>(action.Params);

            _unitOfWork.BeginTransaction();

            try
            {
                if (isNew)
                    objActionInfo.Id = await _repository.AddAsync(objActionInfo);
                else
                {
                    var isUpdated = await _repository.UpdateAsync(objActionInfo);
                    if (!isUpdated) ErrorHandling.ThrowUpdateFailedException(objActionInfo);

                    await _repository.DeleteByScopeAsync<ActionParamInfo>(objActionInfo.Id);
                }

                await _repository.BulkInsertAsync(actionParams.Select(p => { p.ActionId = objActionInfo.Id; return p; }));

                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();

                throw ex;
            }

            return objActionInfo.Id;
        }

        public async Task<bool> DeleteActionAsync(Guid actionId)
        {
            _unitOfWork.BeginTransaction();

            try
            {
                var result = await _repository.DeleteAsync<ActionInfo>(actionId);

                _unitOfWork.Commit();

                return result;
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
                var items = await GetScenarioActionsAsync(context.Get<Guid>("ScenarioId"));

                return new ExportResponse()
                {
                    Result = items,
                    IsSuccess = true
                };
            }
            else if (context.Scope == ImportExportScope.Module)
            {
                var items = await GetActionsAsync(context.Get<Guid>("ModuleId"));

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
            var actions = JsonConvert.DeserializeObject<IEnumerable<ActionInfo>>(items[0].ToString());
            var actionsParams = JsonConvert.DeserializeObject<IEnumerable<ActionParamInfo>>(items[1].ToString());

            if (context.Scope == ImportExportScope.ScenarioFullComponents)
            {
                await BulkInsertActionsAndParamsAsync(actions, actionsParams);
            }
            else if (context.Scope == ImportExportScope.Module)
            {
                var moduleId = (Guid)context.DataTrack["ModuleId"];

                await AddActionsAndParamsAsync(moduleId, actions, actionsParams, context);
            }

            return new ImportResponse()
            {
                IsSuccess = true
            };
        }

        private async Task<object> GetScenarioActionsAsync(Guid scenarioId)
        {
            var actions = new List<ActionInfo>();
            var actionsParams = new List<ActionParamInfo>();

            var moduleIds = await _repository.GetColumnsValueAsync<ModuleInfo, Guid>("Id", "ScenarioId", scenarioId);
            foreach (var moduleId in moduleIds)
            {
                var items = await _repository.GetByScopeAsync<ActionInfo>(moduleId);
                actions.AddRange(items);

                foreach (var action in items)
                {
                    actionsParams.AddRange(await _repository.GetByScopeAsync<ActionParamInfo>(action.Id));
                }
            }

            return new List<object>() { actions, actionsParams };
        }

        private async Task<object> GetActionsAsync(Guid moduleId)
        {
            var actions = await _repository.GetByScopeAsync<ActionInfo>(moduleId);
            var actionsParams = new List<ActionParamInfo>();

            foreach (var action in actions)
            {
                actionsParams.AddRange(await _repository.GetByScopeAsync<ActionParamInfo>(action.Id));
            }

            return new List<object>() { actions, actionsParams };
        }

        private async Task BulkInsertActionsAndParamsAsync(IEnumerable<ActionInfo> actions, IEnumerable<ActionParamInfo> actionsParams)
        {
            await _repository.BulkInsertAsync<ActionInfo>(actions);
            await _repository.BulkInsertAsync<ActionParamInfo>(actionsParams);
        }

        private async Task AddActionsAndParamsAsync(Guid moduleId, IEnumerable<ActionInfo> actions, IEnumerable<ActionParamInfo> actinsParams, ImportContext context)
        {
            var dict = new Dictionary<Guid, Guid>();
            foreach (var action in actions.OrderBy(f => f.ParentId))
            {
                var oldActionId = action.Id;

                action.ModuleId = moduleId;
                action.Id = Guid.NewGuid();
                if (action.FieldId.HasValue && context.DataTrack.TryGetValue(action.FieldId.Value.ToString(), out var fieldId))
                    action.FieldId = (Guid)fieldId;

                if (action.ParentId.HasValue)
                    action.ParentId = dict[action.ParentId.Value];

                var actionId = await _repository.AddAsync<ActionInfo>(action);

                dict.Add(oldActionId, actionId);
            }

            foreach (var param in actinsParams)
            {
                param.ActionId = dict[param.ActionId];
                param.Id = Guid.NewGuid();

                await _repository.AddAsync<ActionParamInfo>(param);
            }
        }

        #endregion
    }
}
