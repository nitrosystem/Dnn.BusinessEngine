using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Core.Security;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Data.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Extension;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Shared.Globals;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataService.Extension
{
    public class ExtensionService : IExtensionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryBase _repository;

        public ExtensionService(IUnitOfWork unitOfWork, IRepositoryBase repository)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
        }

        public async Task<IEnumerable<ExtensionViewModel>> GetExtensionsViewModelAsync()
        {
            var extensions = await _repository.GetAllAsync<ExtensionInfo>("ExtensionName");

            return HybridMapper.MapCollection<ExtensionInfo, ExtensionViewModel>(extensions);
        }

        public IEnumerable<string> GetAvailableExtensionsViewModel()
        {
            var availableExtensions = new List<string>();

            var basePath = Constants.MapPath("~/DesktopModules/BusinessEngine/install");
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

        public async Task<string> GetCurrentVersionExtensionsAsync(string extensionName)
        {
            return await _repository.GetColumnValueAsync<ExtensionInfo, string>("Version", "ExtensionName", extensionName);
        }

        public async Task SaveExtensionAsync(ExtensionViewModel extension, bool isNewExtension)
        {
            var objExtensionInfo = HybridMapper.Map<ExtensionViewModel, ExtensionInfo>(extension);

            if (isNewExtension)
                await _repository.AddAsync<ExtensionInfo>(objExtensionInfo);
            else
            {
                var isUpdated = await _repository.UpdateAsync<ExtensionInfo>(objExtensionInfo);
                if (!isUpdated) ErrorHandling.ThrowUpdateFailedException(objExtensionInfo);
            }
        }
    }
}
