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

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataServices.Contracts
{
    public interface IModuleVariableService
    {
        Task<IEnumerable<ModuleVariableViewModel>> GetModuleVariablesViewModelAsync(Guid moduleId);
        Task<IEnumerable<ModuleVariableDto>> GetModuleVariablesDtoAsync(Guid moduleId);
        Task<Guid> SaveModuleVariablesAsync(ModuleVariableViewModel variale, bool isNew);
        Task<bool> DeleteModuleVariablesAsync(Guid moduleId);
    }
}
