using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Contracts
{
    public interface IMergeResourcesService
    {
        Task<(string Scripts, string Styles)> MergeResourcesAsync(ModuleDto module, int userId, IEnumerable<ModuleResourceDto> resources, IEngineNotifier engineNotifier);
    }
}
