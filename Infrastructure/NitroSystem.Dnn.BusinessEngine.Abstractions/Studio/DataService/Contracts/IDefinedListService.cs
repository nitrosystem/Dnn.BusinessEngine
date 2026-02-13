using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Base;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts
{
    public interface IDefinedListService
    {
        Task<IEnumerable<DefinedListViewModel>> GetDefinedLists(Guid scenarioId);
        Task<DefinedListViewModel> GetDefinedListByListName(string listName, string sortBy = "ViewOrder");
        Task<Guid> SaveDefinedList(DefinedListViewModel definedList, bool isNew);
    }
}
