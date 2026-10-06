using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.InstallExtension.Contracts
{
public    interface IInstallExtensionRunner
    {
        Task<InstallExtensionResponse> RunAsync(InstallExtensionRequest request);
    }
}
