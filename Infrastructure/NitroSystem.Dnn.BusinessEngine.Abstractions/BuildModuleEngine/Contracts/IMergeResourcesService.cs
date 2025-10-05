using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.BuildModuleEngine.Dto;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.BuildModuleEngine.Contracts
{
    public interface IMergeResourcesService
    {
        Task<(string Scripts, string Styles)> MergeResourcesAsync(IEnumerable<ModuleResourceDto> resources);
    }
}
