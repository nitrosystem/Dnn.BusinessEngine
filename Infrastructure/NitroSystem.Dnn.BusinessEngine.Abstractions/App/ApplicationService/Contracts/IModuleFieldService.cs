using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Dto;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Contracts
{
    public interface IModuleFieldService
    {
        Task<IEnumerable<ModuleFieldDto>> GetFieldsDtoAsync(Guid moduleId);
        Task<ModuleFieldDto> GetFieldDtoAsync(Guid fieldId, bool includeDataSource = false);
        Task<ModuleFieldDto> GetFieldDtoAsync(Guid fieldId, bool includeDataSource = false, bool includeSettings = false);
    }
}
