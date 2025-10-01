using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.App.Services.Dto;
using NitroSystem.Dnn.BusinessEngine.App.Services.Enums;

namespace NitroSystem.Dnn.BusinessEngine.App.Services.Contracts
{
    public interface IModuleService
    {
        Task<ModuleDto> GetModuleViewModelAsync(Guid moduleId);
        Task<IEnumerable<ModuleFieldDto>> GetFieldsViewModelAsync(Guid moduleId);
        Task<IEnumerable<ModuleVariableDto>> GetModuleVariables(Guid moduleId, ModuleVariableScope scope);
        Task<IEnumerable<ModuleClientVariableDto>> GetModuleClientVariables(Guid moduleId);
    }
}
