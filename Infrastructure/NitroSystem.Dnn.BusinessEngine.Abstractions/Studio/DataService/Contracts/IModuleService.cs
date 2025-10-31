using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Module;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts
{
    public interface IModuleService
    {
        Task<ModuleViewModel> GetModuleViewModelAsync(Guid moduleId);
        Task<IEnumerable<ModuleViewModel>> GetModulesViewModelAsync(Guid scenarioId);
        Task<Guid> GetScenarioIdAsync(Guid moduleId);
        Task<Guid> SaveModuleAsync(ModuleViewModel module, bool isNew);
        Task<bool> DeleteModuleAsync(Guid moduleId);
        Task<bool?> IsValidModuleNameAsync(Guid scenarioId, Guid? moduleId, string moduleName);

        Task<ModuleDto> GetDataForModuleBuildingAsync(Guid moduleId);
        Task DeleteModuleResourcesAsync(Guid moduleId);
        Task BulkInsertModuleOutputResourcesAsync(int? sitePageId,IEnumerable<ModuleResourceDto> resources);
    }
}
