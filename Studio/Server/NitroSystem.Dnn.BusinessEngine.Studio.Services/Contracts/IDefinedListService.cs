using System;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Base;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts
{
    public interface IDefinedListService
    {
        Task<DefinedListViewModel> GetDefinedListByListName(string listName);
        Task<Guid> SaveDefinedList(DefinedListViewModel definedList, bool isNew);
    }
}
