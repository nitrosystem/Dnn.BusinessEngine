using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Models;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts
{
    public interface IModuleVariableService 
    {
        Task<IEnumerable<ModuleVariableViewModel>> GetVariablesViewModelAsync(Guid moduleId);
        Task<IEnumerable<ModuleVariableListItem>> GetVariablesListItemAsync(Guid moduleId);
        
        Task<Guid> CreateVariablesAsync(ModuleVariableViewModel variale);
        Task SortVariablesAsync(ItemsOrder postData);
        
        Task<bool> DeleteVariablesAsync(Guid moduleId);
    }
}
