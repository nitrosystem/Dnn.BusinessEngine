using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Action;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts
{
    public interface IActionService
    {
        #region Action Type

        Task<IEnumerable<ActionTypeListItem>> GetActionTypesListItemAsync(params string[] sortBy );
        
        #endregion

        #region Action 
        
        Task<(IEnumerable<ActionViewModel> Items, int TotalCount)> GetActionsViewModelAsync(Guid moduleId, Guid? fieldId, string searchText, string actionType);
        Task<IEnumerable<ActionListItem>> GetActionsListItemAsync(Guid moduleId, Guid? fieldId = null, string sortBy = "ExecuteOrder");
        Task<ActionViewModel> GetActionViewModelAsync(Guid actionId);
        Task<ActionDto> ValidateActionExecuteOrderAsync(Guid moduleId, Guid? fieldId, Guid? parentId, string eventName, int executeOrder);
        Task<Guid> SaveActionAsync(ActionViewModel action);
        Task<bool> DeleteActionAsync(Guid actionId);

        #endregion
    }
}
