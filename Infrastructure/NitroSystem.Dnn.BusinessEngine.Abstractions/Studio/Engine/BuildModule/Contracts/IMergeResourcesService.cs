using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Dto;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Contracts
{
    public interface IMergeResourcesService
    {
        Task<(string Scripts, string Styles)> MergeResourcesAsync(IEnumerable<ModuleResourceDto> resources);
    }
}
