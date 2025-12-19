using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Core.Security;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Views;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Action;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Data.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataService.Action
{
    public class ActionService : IActionService
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
            var objActionInfo = HybridMapper.Map<ActionViewModel, ActionInfo>(action);
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
    }
}
