using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildType.Contracts
{
    public interface IBuildTypeRunner
    {
        Task<BuildTypeResponse> RunAsync(BuildTypeRequest request);
    }
}
