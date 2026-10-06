using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Module;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts
{
    public interface IModuleLibraryAndResourceService
    {
        #region  Custom Library

        Task<IEnumerable<ModuleCustomLibraryViewModel>> GetCustomLibrariesViewModelAsync(Guid moduleId);
        
        Task<Guid> CreateCustomLibraryAsync(ModuleCustomLibraryViewModel library);
        Task SortCustomLibrariesAsync(ItemsOrder item);
        
        Task<bool> DeleteCustomLibraryAsync(Guid moduleId);

        #endregion

        #region  Custom Resource

        Task<IEnumerable<ModuleCustomResourceViewModel>> GetCustomResourcesViewModelAsync(Guid moduleId);
        
        Task<Guid> CreateCustomResourceAsync(ModuleCustomResourceViewModel resource);
        Task SortCustomResourcesAsync(ItemsOrder item);
      
        Task<bool> DeleteCustomResourceAsync(Guid moduleId);

        #endregion
    }
}
