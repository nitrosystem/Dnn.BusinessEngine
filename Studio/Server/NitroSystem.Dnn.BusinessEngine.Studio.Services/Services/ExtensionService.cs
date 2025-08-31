using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Tokens;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Common.IO;
using NitroSystem.Dnn.BusinessEngine.Common.Reflection;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Cashing;
using NitroSystem.Dnn.BusinessEngine.Core.Security;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.Dto;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Dto;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Enums;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Mapping;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using NitroSystem.Dnn.BusinessEngine.Core.Mapper;
using System.Runtime.Remoting.Messaging;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using DotNetNuke.Data;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Extension;
using System.IO;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.Contracts;
using System.Threading;
using System.Resources;
using NitroSystem.Dnn.BusinessEngine.Core.Extensions.Manifest;
using System.Security.Cryptography;
using NitroSystem.Dnn.BusinessEngine.Core.General;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Services
{
    public class ExtensionService : IExtensionService
    {
        private readonly IGlobalService _globalService;
        private readonly IExtensionManager _extensionManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryBase _repository;

        public ExtensionService(IUnitOfWork unitOfWork, IRepositoryBase repository, IGlobalService globalService, IExtensionManager extensionManager)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
            _globalService = globalService;
            _extensionManager = extensionManager;
        }

        #region Extension Services

        public async Task<IEnumerable<ExtensionViewModel>> GetExtensionsViewModelAsync()
        {
            var extensions = await _repository.GetAllAsync<ExtensionInfo>("ExtensionName");

            return extensions.Select(source =>
            {
                return HybridMapper.MapWithConfig<ExtensionInfo, ExtensionViewModel>(
                    source, (src, dest) =>
                    {

                    });
            });
        }

        public async Task<ExtensionInstallationResultDto> InstallExtensionAsync(Guid scenarioId, string extensionZipFile)
        {
            var result = new ExtensionInstallationResultDto();

            var scenarioName = await _globalService.GetScenarioNameAsync(scenarioId);

            var unzipedPath = PortalSettings.Current.HomeSystemDirectoryMapPath + @"business-engine\temp\" + scenarioName + @"\" + Path.GetFileNameWithoutExtension(extensionZipFile);
            ZipUtil.Unzip(extensionZipFile, unzipedPath);

            var files = Directory.GetFiles(unzipedPath);
            var manifestFile = files.FirstOrDefault(f => Path.GetFileName(f) == "manifest.json");

            var manifestJson = await FileUtil.GetFileContentAsync(manifestFile);
            var extension = JsonConvert.DeserializeObject<ExtensionManifest>(manifestJson);

            var objExtensionInfo = await _repository.GetAsync<ExtensionInfo>(extension.Id);

            extension.IsNewExtension = objExtensionInfo == null;

            _unitOfWork.BeginTransaction();

            try
            {
                await SaveExtensionAsync(extension, objExtensionInfo?.Version);

                result = await _extensionManager.InstallExtension(extension, _unitOfWork, objExtensionInfo?.Version, unzipedPath);

                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();

                throw ex;
            }
            finally
            {
                // Remove temp folder & files
                File.Delete(extensionZipFile);
                Directory.Delete(unzipedPath, true);
            }

            return result;
        }

        private async Task SaveExtensionAsync(ExtensionManifest extension, string oldVersionn)
        {
            var objExtensionInfo = HybridMapper.MapWithConfig<ExtensionManifest, ExtensionInfo>(
                extension, (src, dest) =>
                {
                    dest.ExtensionImage = dest.ExtensionImage?.ReplaceFrequentTokens();
                    dest.Owner = JsonConvert.SerializeObject(extension.Owner);
                    dest.Resources = JsonConvert.SerializeObject(extension.Resources);
                    dest.Assemblies = JsonConvert.SerializeObject(extension.Assemblies);
                    dest.SqlProviders = JsonConvert.SerializeObject(extension.SqlProviders);
                });

            if (extension.IsNewExtension)
                await _repository.AddAsync<ExtensionInfo>(objExtensionInfo, extension.IsNewExtension);
            else
            {
                if (new Version(oldVersionn) > new Version(extension.Version)) throw new Exception("The installed extension should not be larger than the new extension");

                var isUpdated = await _repository.UpdateAsync<ExtensionInfo>(objExtensionInfo);
                if (!isUpdated) ErrorHandling.ThrowUpdateFailedException(objExtensionInfo);
            }
        }

        public async Task<bool> UninstallExtension(Guid extensionId)
        {
            return false;
        }

        #endregion
    }
}
