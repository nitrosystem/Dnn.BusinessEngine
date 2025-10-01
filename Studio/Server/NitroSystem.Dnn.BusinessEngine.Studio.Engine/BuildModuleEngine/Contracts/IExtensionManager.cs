using DotNetNuke.Entities.Portals;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.Contracts
{
    public interface IExtensionManager
    {
        Task<ExtensionInstallationResultDto> InstallExtension(ExtensionManifest extension, IUnitOfWork unitOfWork, string oldVersion, string unzipedPath);
    }
}
