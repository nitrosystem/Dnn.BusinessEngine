using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.BuildModuleEngine.Dto;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.BuildModuleEngine.Contracts
{
    public interface IBuildLayoutService
    {
        Task<string> BuildLayoutAsync(string moduleLayoutTemplate, IEnumerable<ModuleFieldDto> fields);
    }
}
