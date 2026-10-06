using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Web.Dto;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Contracts
{
    public interface IModuleService
    {
        Task<ModuleDto> GetModuleViewModelAsync(Guid moduleId);
        Task<string> GetModuleNameAsync(Guid moduleId);
        Task<string> GetScenarioNameAsync(Guid moduleId);
        ModuleLiteDto GetModuleLiteData(int? siteModuleId, Guid? moduleId = null);
        IReadOnlyList<ModuleOutputResourceDto> GetModuleOutputResources(int sitePageId,string moduleIds);
    }
}
