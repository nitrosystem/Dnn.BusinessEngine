using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Contracts
{
    public interface IModuleVariableService
    {
        Task<IEnumerable<ModuleVariableDto>> GetVariablesAsync(Guid moduleId, ModuleVariableScope fromScope, ModuleVariableScope toScope);
        Task<IEnumerable<ModuleClientVariableDto>> GetClientVariablesAsync(Guid moduleId);
    }
}
