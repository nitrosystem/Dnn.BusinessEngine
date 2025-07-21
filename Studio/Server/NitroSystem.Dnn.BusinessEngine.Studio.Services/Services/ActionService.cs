using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Tokens;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Common.IO;
using NitroSystem.Dnn.BusinessEngine.Common.Reflection;
using NitroSystem.Dnn.BusinessEngine.Studio.ApplicationActions.Mapping;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Cashing;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.Dto;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Dto;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Dto.Module;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Enums;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Mapping;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using NitroSystem.Dnn.BusinessEngine.Core.Mapper;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Dto.Global;
using System.Collections.Concurrent;
using System.Threading;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Services
{
    public class ActionService : IActionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly IRepositoryBase _repository;

        private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> _actionLocks = new ConcurrentDictionary<Guid, SemaphoreSlim>();

        public ActionService(IUnitOfWork unitOfWork, ICacheService cacheService, IRepositoryBase repository)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _repository = repository;
        }

        public async Task<IEnumerable<ActionTypeViewModel>> GetActionTypesViewModelAsync(string sortBy = "ViewOrder")
        {
            var actionTypes = await _repository.GetAllAsync<ActionTypeView>(sortBy);

            return actionTypes.Select(actionType =>
            {
                return HybridMapper.MapWithConfig<ActionTypeView, ActionTypeViewModel>(actionType, (src, dest) =>
                {
                    dest.Icon = (dest.Icon ?? string.Empty).ReplaceFrequentTokens();
                });
            });
        }

        #region Action Services

        public async Task<IEnumerable<ActionLiteDto>> GetActionsLiteDtoAsync(Guid moduleId, string sortBy = "ViewOrder")
        {
            var actions = await _repository.GetByScopeAsync<ActionInfo>(moduleId, sortBy);

            return actions.Select(action =>
            {
                return HybridMapper.Map<ActionInfo, ActionLiteDto>(action);
            });
        }

        public async Task<(IEnumerable<ActionViewModel> Items, int TotalCount)> GetActionsViewModelAsync(
            Guid moduleId, Guid? fieldId, int pageIndex, int pageSize, string searchText, string actionType, string sortBy)
        {
            var results = await _repository.ExecuteStoredProcedureAsListWithPagingAsync<ActionView>("BusinessEngine_GetActions",
                        new { ModuleId = moduleId, FieldId = fieldId, SearchText = searchText, ActionType = actionType, PageIndex = pageIndex, PageSize = pageSize, SortBy = sortBy });

            var actions = results.Item1;
            var totalCount = results.Item2;

            return (ActionMapping.MapActionsViewModel(actions, Enumerable.Empty<ActionParamInfo>(), Enumerable.Empty<ActionConditionInfo>()), totalCount);
        }

        public async Task<ActionViewModel> GetActionViewModelAsync(Guid actionId)
        {
            var action = await _repository.GetAsync<ActionView>(actionId);
            var actionsResults = await _repository.GetByScopeAsync<ActionResultInfo>(actionId);
            var actionsConditions = await _repository.GetByScopeAsync<ActionConditionInfo>(actionId);
            var actionsParams = await _repository.GetByScopeAsync<ActionParamInfo>(actionId);

            return ActionMapping.MapActionViewModel(action, actionsResults, actionsConditions, actionsParams);
        }

        public async Task<IEnumerable<ModuleFieldLiteDto>> GetFieldsHaveActionsAsync(Guid moduleId)
        {
            return await _repository.ExecuteStoredProcedureAsListAsync<ModuleFieldLiteDto>("BusinessEngine_GetFieldsHaveActions",
                new { ModuleId = moduleId });
        }

        public async Task<Guid> SaveActionAsync(ActionViewModel action, bool isNew)
        {
            var lockKey = action.Id == Guid.Empty ? Guid.NewGuid() : action.Id;
            var semaphore = _actionLocks.GetOrAdd(lockKey, _ => new SemaphoreSlim(1, 1));

            var actionObjects = ActionMapping.MapActionInfoWithChilds(action);
            var objActionInfo = actionObjects.Action;
            var actionResults = actionObjects.Results;
            var actionConditions = actionObjects.Conditions;
            var actionParams = actionObjects.Params;

            await semaphore.WaitAsync();

            try
            {
                if (isNew)
                    objActionInfo.Id = await _repository.AddAsync<ActionInfo>(objActionInfo);
                else
                {
                    var isUpdated = await _repository.UpdateAsync<ActionInfo>(objActionInfo);
                    if (!isUpdated) ErrorService.ThrowUpdateFailedException(objActionInfo);

                    await _repository.DeleteByScopeAsync<ActionResultInfo>(objActionInfo.Id);
                    await _repository.DeleteByScopeAsync<ActionConditionInfo>(objActionInfo.Id);
                    await _repository.DeleteByScopeAsync<ActionParamInfo>(objActionInfo.Id);
                }

                await _repository.BulkInsertAsync<ActionResultInfo>(actionResults.Select(p => { p.ActionId = objActionInfo.Id; return p; }));
                await _repository.BulkInsertAsync<ActionConditionInfo>(actionConditions.Select(p => { p.ActionId = objActionInfo.Id; return p; }));
                await _repository.BulkInsertAsync<ActionParamInfo>(actionParams.Select(p => { p.ActionId = objActionInfo.Id; return p; }));
            }
            finally
            {
                semaphore.Release();
                _actionLocks.TryRemove(lockKey, out _);
            }

            return objActionInfo.Id;
        }

        public async Task<bool> DeleteActionAsync(Guid id)
        {
            return await _repository.DeleteAsync<ActionInfo>(id);
        }

        #endregion
    }
}
