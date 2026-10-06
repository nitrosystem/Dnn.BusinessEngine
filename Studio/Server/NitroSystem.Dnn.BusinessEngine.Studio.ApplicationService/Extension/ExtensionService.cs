using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Extension;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Extension;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.InstallExtension.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.InstallExtension;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.InstallExtension.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Extension;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Shared.Extensions;

namespace NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.Extension
{
    public class ExtensionService : IExtensionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISql _sql;
        private readonly IInstallExtensionRunner _installExtensionRunner;
        private readonly IExtensionRepository _extensionRepository;

        public ExtensionService(IUnitOfWork unitOfWork, ISql sql, IInstallExtensionRunner installExtensionRunner, IExtensionRepository extensionRepository)
        {
            _unitOfWork = unitOfWork;
            _sql = sql;
            _installExtensionRunner = installExtensionRunner;
            _extensionRepository = extensionRepository;
        }

        public async Task<string> GetCurrentVersionExtensionsAsync(string extensionName)
        {
            return await _extensionRepository.GetVersionAsync(extensionName);
        }

        public async Task<IEnumerable<ExtensionViewModel>> GetExtensionsViewModelAsync()
        {
            var extensions = await _extensionRepository.GetsAsync("ExtensionName");
            return HybridMapper.MapCollection<ExtensionInfo, ExtensionViewModel>(extensions);
        }

        public IEnumerable<string> GetAvailableExtensionsViewModel()
        {
            var availableExtensions = new List<string>();

            var basePath = GlobalHelper.MapPath("~/DesktopModules/BusinessEngine/install");
            if (Directory.Exists(basePath))
            {
                var zipFiles = Directory.EnumerateFiles(basePath, "*.zip").ToList();
                foreach (var filePath in zipFiles)
                {
                    availableExtensions.Add(Path.GetFileName(filePath));
                }
            }

            return availableExtensions;
        }

        public (ExtensionManifest Manifest, string ExtractPath) GetExtensionManifest(string filename, string basePath)
        {
            string tempPath = GetOrCreateExtensionTempPath(basePath);
            string extractPath = tempPath + Path.GetFileNameWithoutExtension(filename);
            ZipUtil.Unzip(filename, extractPath, true);

            var files = Directory.GetFiles(extractPath);
            var manifestFile = files.FirstOrDefault(f => Path.GetFileName(f) == "manifest.json");
            var manifestJson = FileUtil.GetFileContent(manifestFile);
            var manifest = JsonConvert.DeserializeObject<ExtensionManifest>(manifestJson);

            //Show logo from manifest directory when installing package
            manifest.Logo = manifest.Logo?.ReplaceFrequentTokens();
            var logoFilePath = Path.Combine(extractPath, "logo.png");
            if (File.Exists(logoFilePath))
                manifest.Logo = UrlHelper.ConvertToAbsoluteVirtualPath(logoFilePath);

            return (manifest, extractPath);
        }

        public (ExtensionManifest Manifest, string ExtractPath) GetAvailableExtension(string extensionFilename, string basePath)
        {
            var installPath = GlobalHelper.MapPath("~/DesktopModules/BusinessEngine/install");
            var filename = Path.Combine(installPath, extensionFilename);
            var items = GetExtensionManifest(filename, basePath);

            return items;
        }

        public async Task InstallExtension(InstallExtensionRequest request)
        {
            request.ModulePath = GlobalHelper.MapPath("~/DesktopModules/BusinessEngine");

            var files = Directory.GetFiles(request.ExtractPath);
            var manifestFile = files.FirstOrDefault(f => Path.GetFileName(f) == "manifest.json");
            var manifestJson = await FileUtil.GetFileContentAsync(manifestFile);

            request.Manifest = JsonConvert.DeserializeObject<ExtensionManifest>(manifestJson);
            var response = await _installExtensionRunner.RunAsync(request);

            if (request.IsAvailableExtension)
            {
                var file = GlobalHelper.MapPath($"~/DesktopModules/BusinessEngine/install/{request.AvailableExtensionFileName}");
                if (File.Exists(file)) File.Delete(file);
            }
        }

        public async Task CreateExtensionAsync(ExtensionViewModel extension, bool isNewExtension)
        {
            var objExtensionInfo = HybridMapper.Map<ExtensionViewModel, ExtensionInfo>(extension);
            if (isNewExtension)
            {
                await _extensionRepository.AddAsync(objExtensionInfo);
            }
            else
            {
                await _extensionRepository.UpdateAsync(objExtensionInfo);
            }
        }

        public string GetOrCreateExtensionTempPath(string basePath)
        {
            var uploadPath = Path.Combine(basePath, @"temp\extensions\");
            if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);
            return uploadPath;
        }
    }
}
