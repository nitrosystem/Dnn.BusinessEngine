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
        Task<Guid> SaveModuleAsync(ModuleViewModel module, bool isNew);
        Task<bool> DeleteModuleAsync(Guid moduleId);
        Task<bool?> IsValidModuleName(Guid scenarioId, Guid? moduleId, string moduleName);

        Task<ModuleTemplateViewModel> GetModuleTemplateViewModelAsync(Guid moduleId);
        Task<bool> UpdateModuleTemplateAsync(ModuleTemplateViewModel module);

        Task<ModuleDto> GetDataForModuleBuildingAsync(Guid moduleId);
        Task<bool> DeleteModuleResourcesAsync(Guid moduleId);
        Task BulkInsertModuleOutputResources(int? sitePageId,IEnumerable<ModuleResourceDto> resources);
    }
}
