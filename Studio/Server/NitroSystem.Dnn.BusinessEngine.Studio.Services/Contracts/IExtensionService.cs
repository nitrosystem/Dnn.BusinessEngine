using DotNetNuke.Entities.Portals;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.Dto;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts
{
    public interface IExtensionService
    {
        Task<IEnumerable<ExtensionViewModel>> GetExtensionsViewModelAsync();
     
        Task<ExtensionInstallationResultDto> InstallExtensionAsync(Guid scenarioId, string extensionZipFile);
       
        Task<bool> UninstallExtension(Guid extensionId);
    }
}
