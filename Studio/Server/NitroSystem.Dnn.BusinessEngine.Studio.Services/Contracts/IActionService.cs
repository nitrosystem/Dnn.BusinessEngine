using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ListItems;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Action;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts
{
    public interface IActionService
    {
        Task<IEnumerable<ActionTypeListItem>> GetActionTypesListItemAsync(string sortBy = "ViewOrder");
        Task<(IEnumerable<ActionViewModel> Items, int TotalCount)> GetActionsViewModelAsync(Guid moduleId, Guid? fieldId, int pageIndex, int pageSize, string searchText, string actionType, string sortBy);
        Task<ActionViewModel> GetActionViewModelAsync(Guid actionId);
        Task<Guid> SaveActionAsync(ActionViewModel action, bool isNew);
        Task<bool> DeleteActionAsync(Guid actionId);
    }
}
