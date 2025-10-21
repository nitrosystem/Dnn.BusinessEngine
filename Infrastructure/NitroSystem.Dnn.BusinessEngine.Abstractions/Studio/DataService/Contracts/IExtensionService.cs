using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Extension;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.InstallExtension.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts
{
    public interface IExtensionService
    {
        Task<IEnumerable<ExtensionViewModel>> GetExtensionsViewModelAsync();
        IEnumerable<string> GetAvailableExtensionsViewModel();
        Task<string> GetCurrentVersionExtensionsAsync(string extensionName);
        Task<Guid> SaveExtensionAsync(ExtensionManifest extension, IUnitOfWork UnitOfWork);

        //Task<bool> UninstallExtension(Guid extensionId);
    }
}
