using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Contracts
{
    public interface IBuildModuleRunner
    {
        Task<bool> RunAsync(BuildModuleRequest request);
    }
}
