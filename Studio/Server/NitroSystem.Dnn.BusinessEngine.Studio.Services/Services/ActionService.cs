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
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Attributes;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Services
{
    public class ActionService : IActionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly IRepositoryBase _repository;

        public ActionService(IUnitOfWork unitOfWork, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _repository = new RepositoryBase(_unitOfWork, _cacheService);
        }

        #region Action Services

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
            var actionsParams = await _repository.GetByScopeAsync<ActionParamInfo>(actionId);
            var actionsConditions = await _repository.GetByScopeAsync<ActionConditionInfo>(actionId);

            return ActionMapping.MapActionViewModel(action, actionsParams, actionsConditions);
        }

        public async Task<IEnumerable<ModuleFieldLiteDto>> GetFieldsHaveActionsAsync(Guid moduleId)
        {
            return await _repository.ExecuteStoredProcedureAsListAsync<ModuleFieldLiteDto>("BusinessEngine_GetFieldsHaveActions",
                new { ModuleId = moduleId });
        }

        //public async Task<Guid> SaveActionAsync(ActionViewModel action, bool isNew)
        //{
        //    var objActionInfo = ActionMapping.MapActionInfo(action);

        //    if (isNew)
        //        objActionInfo.Id = await _repository.AddAsync<ActionView>(objActionInfo);
        //    else
        //    {
        //        var isUpdated = await _repository.UpdateAsync<ActionView>(objActionInfo);
        //        if (!isUpdated) ErrorService.ThrowUpdateFailedException(objActionInfo);
        //    }

        //    return objActionInfo.Id;
        //}

        //public async Task<bool> DeleteActionAsync(Guid id)
        //{
        //    return await _repository.DeleteAsync<ActionView>(id);
        //}

        #endregion
    }
}
