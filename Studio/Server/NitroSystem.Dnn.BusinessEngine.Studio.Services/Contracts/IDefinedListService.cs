using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts
{
    public interface IDefinedListService
    {
        Task<DefinedListViewModel> GetDefinedListByFieldId(Guid fieldId);

        Task<Guid> SaveDefinedList(DefinedListViewModel definedList, bool isNew);
    }
}
