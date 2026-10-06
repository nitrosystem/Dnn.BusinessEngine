using System.Collections.Generic;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.InstallExtension;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.InstallExtension.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Extension;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts
{
    public interface IExtensionService
    {
        Task<string> GetCurrentVersionExtensionsAsync(string extensionName);
        Task<IEnumerable<ExtensionViewModel>> GetExtensionsViewModelAsync();
        IEnumerable<string> GetAvailableExtensionsViewModel();
        (ExtensionManifest Manifest, string ExtractPath) GetExtensionManifest(string filename, string basePath);
        (ExtensionManifest Manifest, string ExtractPath) GetAvailableExtension(string extensionFilename, string basePath);

        Task InstallExtension(InstallExtensionRequest request);
        Task CreateExtensionAsync(ExtensionViewModel extension, bool isNewExtension);
        string GetOrCreateExtensionTempPath(string basePath);
    }
}
