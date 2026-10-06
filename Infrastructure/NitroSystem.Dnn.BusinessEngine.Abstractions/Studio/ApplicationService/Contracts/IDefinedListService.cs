using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.DefinedList;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts
{
    public interface IDefinedListService
    {
        Task<IEnumerable<DefinedListViewModel>> GetDefinedListsViewModelAsync(Guid scenarioId);
        Task<DefinedListViewModel> GetDefinedListViewModelByListNameAsync(string listName, string sortBy = "ViewOrder");
        
        Task<Guid> CreateDefinedListAsync(DefinedListViewModel definedList);
    }
}
