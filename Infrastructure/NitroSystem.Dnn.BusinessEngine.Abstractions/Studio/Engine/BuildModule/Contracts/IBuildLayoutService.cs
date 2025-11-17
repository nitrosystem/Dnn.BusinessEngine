using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Dto;
using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Contracts
{
    public interface IBuildLayoutService
    {
        Task<string> BuildLayoutAsync(Guid moduleId, int userId, string moduleLayoutTemplate, IEnumerable<ModuleFieldDto> fields);
    }
}
