using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Studio.DataServices.ListItems;
using NitroSystem.Dnn.BusinessEngine.Studio.DataServices.ViewModels.Action;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataServices.Contracts
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
