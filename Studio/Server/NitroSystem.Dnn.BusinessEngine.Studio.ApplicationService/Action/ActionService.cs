using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Action;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Action;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Views.Action;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Action;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Export;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Import;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;

namespace NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.Action
{
    public class ActionService : IActionService, IExportable, IImportable
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IActionTypeRepository _actionTypeRepository;
        private readonly IActionRepository _actionRepository;
        private readonly IActionParamRepository _actionParamRepository;

        public ActionService(
            IUnitOfWork unitOfWork,
            IActionTypeRepository actionTypeRepository,
            IActionRepository actionRepository,
            IActionParamRepository actionParamRepository)
        {
            _unitOfWork = unitOfWork;
            _actionTypeRepository = actionTypeRepository;
            _actionRepository = actionRepository;
            _actionParamRepository = actionParamRepository;
        }

        #region Action Type

        public async Task<IEnumerable<ActionTypeListItem>> GetActionTypesListItemAsync(params string[] sortBy)
        {
            var actionTypes = await _actionTypeRepository.GetsAsync(sortBy);
            return HybridMapper.MapCollection<ActionTypeInfo, ActionTypeListItem>(actionTypes);
        }

        #endregion

        #region Action

        public async Task<(IEnumerable<ActionViewModel> Items, int TotalCount)> GetActionsViewModelAsync(
            Guid moduleId, Guid? fieldId, string searchText, string actionType)
        {
            var results = await _actionRepository.GetsAsync(moduleId, fieldId, searchText, actionType);
            var result = HybridMapper.MapWithChildren<ActionView, ActionViewModel, ActionParamInfo, ActionParamViewModel>(
               parents: results.Actions,
               children: results.Params,
               parentKeySelector: p => p.Id,
               childKeySelector: c => c.ActionId,
               assignChildren: (parent, childs) => parent.Params = childs
            );

            return (result, results.TotalCount);
        }

        public async Task<IEnumerable<ActionListItem>> GetActionsListItemAsync(Guid moduleId, Guid? fieldId, string sortBy = "ExecuteOrder")
        {
            var actions = await _actionRepository.GetsAsync(moduleId, fieldId, sortBy);
            return HybridMapper.MapCollection<ActionInfo, ActionListItem>(actions);
        }

        public async Task<ActionViewModel> GetActionViewModelAsync(Guid actionId)
        {
            var action = await _actionRepository.GetViewAsync(actionId);
            var actionParams = await _actionParamRepository.GetsAsync(actionId);

            return HybridMapper.MapWithChildren<ActionView, ActionViewModel, ActionParamInfo, ActionParamViewModel>(
               source: action,
               children: actionParams,
               assignChildren: (parent, childs) => parent.Params = childs
           );
        }

        public async Task<ActionDto> ValidateActionExecuteOrderAsync(Guid moduleId, Guid? fieldId, Guid? parentId, string eventName, int executeOrder)
        {
            var actions = await _actionRepository.GetViewsAsync(moduleId, fieldId, parentId, eventName, executeOrder);
            if (actions.Any())
            {
                var action = actions.First();
                return new ActionDto()
                {
                    ModuleId = action.ModuleId,
                    ActionId = action.Id,
                    FieldId = action.FieldId,
                    FieldType = action.FieldType,
                    ActionName = action.ActionName
                };
            }

            return null;
        }

        public async Task<Guid> SaveActionAsync(ActionViewModel action)
        {
            var objActionInfo = HybridMapper.Map<ActionViewModel, ActionInfo>(action, (src, dest) =>
                                    dest.ParentId = src.Event != "OnActionCompleted" && src.ParentId.HasValue
                                    ? null
                                    : src.ParentId);
            var actionParams = HybridMapper.MapCollection<ActionParamViewModel, ActionParamInfo>(action.Params);

            _unitOfWork.BeginTransaction();

            try
            {
                if (action.Id == Guid.Empty)
                {
                    objActionInfo.Id = await _actionRepository.AddAsync(objActionInfo);
                }
                else
                {
                    await _actionRepository.UpdateAsync(objActionInfo);

                    //Delete old ActionParams
                    await _actionParamRepository.DeletesAsync(objActionInfo.Id);
                }

                //Add new ActionParams
                await _actionParamRepository.BulkInsertAsync(actionParams.Select(p => { p.ActionId = objActionInfo.Id; return p; }).ToList());

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
                var result = await _actionRepository.DeleteAsync(actionId);
                _unitOfWork.Commit();
                return result;
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                throw ex;
            }
        }

        #endregion

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
            var actions = JsonConvert.DeserializeObject<IReadOnlyList<ActionInfo>>(items[0].ToString());
            var actionsParams = JsonConvert.DeserializeObject<IReadOnlyList<ActionParamInfo>>(items[1].ToString());

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
            var actions = await _actionRepository.GetsByAncestorAsync(scenarioId);
            var actionsParams = new List<ActionParamInfo>();
            foreach (var action in actions)
            {
                actionsParams.AddRange(await _actionParamRepository.GetsAsync(action.Id));
            }

            return new List<object>() { actions, actionsParams };
        }

        private async Task<object> GetActionsAsync(Guid moduleId)
        {
            var actions = await _actionRepository.GetsAsync(moduleId);
            var actionsParams = await _actionParamRepository.GetsByAncestorAsync(moduleId);
            return new List<object>() { actions, actionsParams };
        }

        private async Task BulkInsertActionsAndParamsAsync(IReadOnlyList<ActionInfo> actions, IReadOnlyList<ActionParamInfo> actionsParams)
        {
            await _actionRepository.BulkInsertAsync(actions);
            await _actionParamRepository.BulkInsertAsync(actionsParams);
        }

        private async Task AddActionsAndParamsAsync(Guid moduleId, IReadOnlyList<ActionInfo> actions, IReadOnlyList<ActionParamInfo> actinsParams, ImportContext context)
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

                var actionId = await _actionRepository.AddAsync(action);

                dict.Add(oldActionId, actionId);
            }

            foreach (var param in actinsParams)
            {
                param.ActionId = dict[param.ActionId];
                param.Id = Guid.NewGuid();
                await _actionParamRepository.AddAsync(param);
            }
        }

        #endregion
    }
}
