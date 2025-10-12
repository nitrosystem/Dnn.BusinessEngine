using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Dto;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Contracts
{
    public interface IBuildLayoutService
    {
        Task<string> BuildLayoutAsync(string moduleLayoutTemplate, IEnumerable<ModuleFieldDto> fields);
    }
}
