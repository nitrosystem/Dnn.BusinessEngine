using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataServices.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataServices.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataServices.ViewModels.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataServices.ViewModels.Template;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataServices.Enums;
using System.Linq;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataServices.Contracts
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
