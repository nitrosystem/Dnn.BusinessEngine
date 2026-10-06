using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Action;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Action;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Views.Action;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Action;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;

namespace NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.Action
{
    public class ActionService : IActionService
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
                await _actionParamRepository.BulkInsertAsync(actionParams.Select(p => { p.ActionId = objActionInfo.Id; return p; }));

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
    }
}
