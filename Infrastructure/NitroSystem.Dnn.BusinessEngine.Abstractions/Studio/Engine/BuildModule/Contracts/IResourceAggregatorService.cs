using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Contracts
{
    public interface IResourceAggregatorService
    {
        Task<BuildModuleResponse> FinalizeResourcesAsync(BuildModuleRequest request, EngineContext context);
    }
}
