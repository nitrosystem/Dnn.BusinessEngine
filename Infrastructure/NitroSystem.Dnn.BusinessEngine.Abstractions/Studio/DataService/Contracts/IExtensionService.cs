using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Extension;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts
{
    public interface IExtensionService
    {
        Task<IEnumerable<ExtensionViewModel>> GetExtensionsViewModelAsync();
        IEnumerable<string> GetAvailableExtensionsViewModel();
        Task<string> GetCurrentVersionExtensionsAsync(string extensionName);
        Task<Guid> SaveExtensionAsync(ExtensionViewModel extension, bool isNewExtension);
    }
}
