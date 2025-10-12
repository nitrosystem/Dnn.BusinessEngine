using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataServices.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataServices.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataServices.Contracts
{
    public interface IModuleService
    {
        Task<ModuleDto> GetModuleViewModelAsync(Guid moduleId);
        Task<IEnumerable<ModuleFieldDto>> GetFieldsViewModelAsync(Guid moduleId);
        Task<IEnumerable<ModuleVariableDto>> GetModuleVariables(Guid moduleId, ModuleVariableScope scope);
        Task<IEnumerable<ModuleClientVariableDto>> GetModuleClientVariables(Guid moduleId);
    }
}
