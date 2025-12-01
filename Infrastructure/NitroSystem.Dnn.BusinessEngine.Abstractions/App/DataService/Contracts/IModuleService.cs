using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Web.Dto;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Contracts
{
    public interface IModuleService
    {
        Task<ModuleDto> GetModuleViewModelAsync(Guid moduleId);
        ModuleLiteDto GetModuleLiteData(int? siteModuleId, Guid? moduleId = null);
        Task<string> GetModuleNameAsync(Guid moduleId);
        Task<string> GetScenarioNameAsync(Guid moduleId);

        Task<IEnumerable<ModuleFieldDto>> GetFieldsDtoAsync(Guid moduleId);
        Task<ModuleFieldDto> GetFieldDtoAsync(Guid fieldId, bool includeDataSource = false);
        Task<IEnumerable<ModuleVariableDto>> GetVariables(Guid moduleId, ModuleVariableScope scope);
        Task<IEnumerable<ModuleClientVariableDto>> GetClientVariables(Guid moduleId);
    }
}
