using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Module;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts
{
    public interface IModuleLibraryAndResourceService
    {
        Task<IEnumerable<ModuleCustomLibraryViewModel>> GetModuleCustomLibrariesAsync(Guid moduleId);
        Task<IEnumerable<ModuleCustomResourceViewModel>> GetModuleCustomResourcesAsync(Guid moduleId);
        Task<Guid> SaveModuleCustomLibraryAsync(ModuleCustomLibraryViewModel library, bool isNew);
        Task<Guid> SaveModuleCustomResourceAsync(ModuleCustomResourceViewModel resource, bool isNew);
        Task SortModuleCustomLibraries(LibraryOrResource target, IEnumerable<SortInfo> items);
        Task<bool> DeleteModuleCustomLibraryAsync(Guid moduleId);
        Task<bool> DeleteModuleCustomResourceAsync(Guid moduleId);
    }
}
