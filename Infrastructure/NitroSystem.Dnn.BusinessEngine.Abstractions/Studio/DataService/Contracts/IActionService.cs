using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Action;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts
{
    public interface IActionService
    {
        Task<IEnumerable<ActionTypeListItem>> GetActionTypesListItemAsync(string sortBy = "GroupViewOrder");
        Task<(IEnumerable<ActionViewModel> Items, int TotalCount)> GetActionsViewModelAsync(Guid moduleId, Guid? fieldId, int pageIndex, int pageSize, string searchText, string actionType, string sortBy);
        Task<IEnumerable<ActionListItem>> GetActionsListItemAsync(Guid moduleId, string sortBy = "ViewOrder");
        Task<ActionViewModel> GetActionViewModelAsync(Guid actionId);
        Task<Guid> SaveActionAsync(ActionViewModel action, bool isNew);
        Task<bool> DeleteActionAsync(Guid actionId);
    }
}
