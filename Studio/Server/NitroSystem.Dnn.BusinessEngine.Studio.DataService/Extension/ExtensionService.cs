using DotNetNuke.Entities.Portals;
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
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.InstallExtension.Models;
using NitroSystem.Dnn.BusinessEngine.Shared.Extensions;

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

        public async Task<string> GetCurrentVersionExtensionsAsync(string extensionName)
        {
            return await _repository.GetColumnValueAsync<ExtensionInfo, string>("Version", "ExtensionName", extensionName);
        }

        public async Task<Guid> SaveExtensionAsync(ExtensionManifest extension, IUnitOfWork UnitOfWork)
        {
            var objExtensionInfo = HybridMapper.Map<ExtensionManifest, ExtensionInfo>(extension);

            if (extension.IsNewExtension)
                objExtensionInfo.Id = await _repository.AddAsync<ExtensionInfo>(objExtensionInfo, extension.IsNewExtension);
            else
            {
                var isUpdated = await _repository.UpdateAsync<ExtensionInfo>(objExtensionInfo);
                if (!isUpdated) ErrorHandling.ThrowUpdateFailedException(objExtensionInfo);
            }

            return objExtensionInfo.Id;
        }

        public async Task<bool> UninstallExtension(Guid extensionId)
        {
            return false;
        }
    }
}
