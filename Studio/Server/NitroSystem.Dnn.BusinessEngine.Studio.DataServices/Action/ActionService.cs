using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Core.Security;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Views;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataServices.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataServices.ViewModels.Action;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataServices.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Data.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataServices.Action
{
    public class ActionService : IActionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryBase _repository;

        private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> _actionLocks =
            new ConcurrentDictionary<Guid, SemaphoreSlim>();

        public ActionService(IUnitOfWork unitOfWork, IRepositoryBase repository)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
        }

        public async Task<IEnumerable<ActionTypeListItem>> GetActionTypesListItemAsync(string sortBy = "ViewOrder")
        {
            var actionTypes = await _repository.GetAllAsync<ActionTypeView>(sortBy);

            return HybridMapper.MapCollection<ActionTypeView, ActionTypeListItem>(actionTypes);
        }

        public async Task<(IEnumerable<ActionViewModel> Items, int TotalCount)> GetActionsViewModelAsync(
            Guid moduleId, Guid? fieldId, int pageIndex, int pageSize, string searchText, string actionType, string sortBy)
        {
            var results = await _repository.ExecuteStoredProcedureMultipleAsync<int, ActionView, ActionParamInfo, ActionResultInfo>(
                "dbo.BusinessEngine_Studio_GetActions", "BE_Actions_Studio_GetActions",
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
            var actionResults = results.Item4;

            var builder = new CollectionMappingBuilder<ActionView, ActionViewModel>();

            builder.AddChildAsync<ActionParamInfo, ActionParamViewModel, Guid>(
               source: actionParams,
               parentKey: parent => parent.Id,
               childKey: child => child.ActionId,
               assign: (dest, children) => dest.Params = children
            );

            builder.AddChildAsync<ActionResultInfo, ActionResultViewModel, Guid>(
              source: actionResults,
              parentKey: parent => parent.Id,
              childKey: child => child.ActionId,
              assign: (dest, children) => dest.Results = children
            );

            var result = await builder.BuildAsync(actions);
            return (result, totalCount);
        }

        public async Task<ActionViewModel> GetActionViewModelAsync(Guid actionId)
        {
            var action = await _repository.GetAsync<ActionView>(actionId);
            var actionResults = await _repository.GetByScopeAsync<ActionResultInfo>(actionId);
            var actionParams = await _repository.GetByScopeAsync<ActionParamInfo>(actionId);

            var builder = new CollectionMappingBuilder<ActionView, ActionViewModel>();

            builder.AddChildAsync<ActionParamInfo, ActionParamViewModel, Guid>(
               source: actionParams,
               parentKey: parent => parent.Id,
               childKey: child => child.ActionId,
               assign: (dest, children) => dest.Params = children
            );

            builder.AddChildAsync<ActionResultInfo, ActionResultViewModel, Guid>(
              source: actionResults,
              parentKey: parent => parent.Id,
              childKey: child => child.ActionId,
              assign: (dest, children) => dest.Results = children
            );

            var result = await builder.BuildAsync(action);
            return result;
        }

        public async Task<Guid> SaveActionAsync(ActionViewModel action, bool isNew)
        {
            var lockKey = action.Id == Guid.Empty ? Guid.NewGuid() : action.Id;
            var semaphore = _actionLocks.GetOrAdd(lockKey, _ => new SemaphoreSlim(1, 1));

            var objActionInfo = HybridMapper.Map<ActionViewModel, ActionInfo>(action);
            var actionParams = HybridMapper.MapCollection<ActionParamViewModel, ActionParamInfo>(action.Params);
            var actionResults = HybridMapper.MapCollection<ActionResultViewModel, ActionResultInfo>(action.Results);

            await semaphore.WaitAsync();

            _unitOfWork.BeginTransaction();

            try
            {
                try
                {
                    if (isNew)
                        objActionInfo.Id = await _repository.AddAsync(objActionInfo);
                    else
                    {
                        var isUpdated = await _repository.UpdateAsync(objActionInfo);
                        if (!isUpdated) ErrorHandling.ThrowUpdateFailedException(objActionInfo);

                        await _repository.DeleteByScopeAsync<ActionResultInfo>(objActionInfo.Id);
                        await _repository.DeleteByScopeAsync<ActionParamInfo>(objActionInfo.Id);
                    }

                    await _repository.BulkInsertAsync(actionResults.Select(p => { p.ActionId = objActionInfo.Id; return p; }));
                    await _repository.BulkInsertAsync(actionParams.Select(p => { p.ActionId = objActionInfo.Id; return p; }));

                    _unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    _unitOfWork.Rollback();

                    throw ex;
                }
            }
            finally
            {
                semaphore.Release();
                _actionLocks.TryRemove(lockKey, out _);
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
