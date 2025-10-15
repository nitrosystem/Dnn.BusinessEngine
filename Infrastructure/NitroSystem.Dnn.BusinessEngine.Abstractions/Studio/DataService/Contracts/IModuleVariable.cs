using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Module;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts
{
    public interface IModuleVariableService
    {
        Task<IEnumerable<ModuleVariableViewModel>> GetModuleVariablesViewModelAsync(Guid moduleId);
        Task<IEnumerable<ModuleVariableListItem>> GetModuleVariablesListItemAsync(Guid moduleId);
        Task<Guid> SaveModuleVariablesAsync(ModuleVariableViewModel variale, bool isNew);
        Task<bool> DeleteModuleVariablesAsync(Guid moduleId);
    }
}
