using System;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Base;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts
{
    public interface IDefinedListService
    {
        Task<DefinedListViewModel> GetDefinedListByListName(string listName);
        Task<Guid> SaveDefinedList(DefinedListViewModel definedList, bool isNew);
    }
}
