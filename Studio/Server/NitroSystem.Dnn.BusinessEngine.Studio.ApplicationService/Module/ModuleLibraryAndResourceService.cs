using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Views.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Module;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Export;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Import;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;

namespace NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.Module
{
    public class ModuleLibraryAndResourceService : IModuleLibraryAndResourceService, IExportable, IImportable
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IModuleCustomLibraryRepository _moduleCustomLibraryRepository;
        private readonly IModuleCustomResourceRepository _moduleCustomResourceRepository;

        public ModuleLibraryAndResourceService(IUnitOfWork unitOfWork, IModuleCustomLibraryRepository moduleCustomLibraryRepository, IModuleCustomResourceRepository moduleCustomResourceRepository)
        {
            _unitOfWork = unitOfWork;
            _moduleCustomLibraryRepository = moduleCustomLibraryRepository;
            _moduleCustomResourceRepository = moduleCustomResourceRepository;
        }

        #region Custom Library

        public async Task<IEnumerable<ModuleCustomLibraryViewModel>> GetCustomLibrariesViewModelAsync(Guid moduleId)
        {
            var libraries = await _moduleCustomLibraryRepository.GetsViewAsync(moduleId, "LoadOrder");
            var librariesResources = await _moduleCustomLibraryRepository.GetsResourceViewAsync(moduleId, "LoadOrder");

            return HybridMapper.MapWithChildren<ModuleCustomLibraryView, ModuleCustomLibraryViewModel,
                                                    ModuleCustomLibraryResourceView, ModuleCustomLibraryResourceViewModel>(
                parents: libraries,
                children: librariesResources,
                parentKeySelector: p => p.LibraryId,
                childKeySelector: c => c.LibraryId,
                assignChildren: (parent, childs) => parent.Resources = childs
            );
        }

        public async Task<Guid> CreateCustomLibraryAsync(ModuleCustomLibraryViewModel library)
        {
            var objModuleCustomLibraryInfo = HybridMapper.Map<ModuleCustomLibraryViewModel, ModuleCustomLibraryInfo>(library);
            if (objModuleCustomLibraryInfo.Id == Guid.Empty)
                objModuleCustomLibraryInfo.Id = await _moduleCustomLibraryRepository.AddAsync(objModuleCustomLibraryInfo);
            else
                await _moduleCustomLibraryRepository.UpdateAsync(objModuleCustomLibraryInfo);

            return objModuleCustomLibraryInfo.Id;
        }

        public async Task SortCustomLibrariesAsync(ItemsOrder item)
        {
            _unitOfWork.BeginTransaction();

            try
            {
                int index = 0;
                foreach (var pageId in item.SortedIds)
                {
                    var objModuleCustomLibraryInfo = new ModuleCustomLibraryInfo() { Id = pageId, LoadOrder = index++ };
                    await _moduleCustomLibraryRepository.UpdateAsync(objModuleCustomLibraryInfo, "LoadOrder");
                }

                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                throw ex;
            }
        }

        public async Task<bool> DeleteCustomLibraryAsync(Guid itemId)
        {
            return await _moduleCustomLibraryRepository.DeleteAsync(itemId);
        }

        #endregion

        #region Custom Resource

        public async Task<IEnumerable<ModuleCustomResourceViewModel>> GetCustomResourcesViewModelAsync(Guid moduleId)
        {
            var resources = await _moduleCustomResourceRepository.GetsAsync(moduleId, "LoadOrder");
            return HybridMapper.MapCollection<ModuleCustomResourceInfo, ModuleCustomResourceViewModel>(resources);
        }

        public async Task<Guid> CreateCustomResourceAsync(ModuleCustomResourceViewModel resource)
        {
            var objModuleCustomResourceInfo = HybridMapper.Map<ModuleCustomResourceViewModel, ModuleCustomResourceInfo>(resource);
            if (objModuleCustomResourceInfo.Id == Guid.Empty)
                objModuleCustomResourceInfo.Id = await _moduleCustomResourceRepository.AddAsync(objModuleCustomResourceInfo);
            else
                await _moduleCustomResourceRepository.UpdateAsync(objModuleCustomResourceInfo);

            return objModuleCustomResourceInfo.Id;
        }

        public async Task SortCustomResourcesAsync(ItemsOrder item)
        {
            _unitOfWork.BeginTransaction();

            try
            {
                int index = 0;
                foreach (var pageId in item.SortedIds)
                {
                    var objModuleCustomResourceInfo = new ModuleCustomResourceInfo() { Id = pageId, LoadOrder = index++ };
                    await _moduleCustomResourceRepository.UpdateAsync(objModuleCustomResourceInfo, "LoadOrder");
                }

                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                throw ex;
            }
        }

        public async Task<bool> DeleteCustomResourceAsync(Guid itemId)
        {
            return await _moduleCustomResourceRepository.DeleteAsync(itemId);
        }

        #endregion

        #region Import Export

        public async Task<ExportResponse> ExportAsync(ExportContext context)
        {
            switch (context.Scope)
            {
                case ImportExportScope.ScenarioFullComponents:
                    var itemss = await GetScenarioLibrariesAndResourcesAsync(context.Get<Guid>("ScenarioId"));

                    return new ExportResponse()
                    {
                        Result = itemss,
                        IsSuccess = true
                    };
                case ImportExportScope.Module:
                    var items = await GetLibrariesAndResourcesAsync(context.Get<Guid>("ModuleId"));

                    return new ExportResponse()
                    {
                        Result = items,
                        IsSuccess = true
                    };
                default:
                    return null;
            }
        }

        public async Task<ImportResponse> ImportAsync(string json, ImportContext context)
        {
            switch (context.Scope)
            {
                case ImportExportScope.Module:
                    var moduleId = (Guid)context.DataTrack["ModuleId"];
                    var items = JsonConvert.DeserializeObject<List<object>>(json);

                    if (items[0] as IEnumerable<ModuleCustomLibraryInfo> != null)
                        await AddLibrariesAsync(moduleId, (IReadOnlyList<ModuleCustomLibraryInfo>)items[0]);

                    if (items[1] as IEnumerable<ModuleCustomResourceInfo> != null)
                        await AddResourcesAsync(moduleId, (IReadOnlyList<ModuleCustomResourceInfo>)items[1]);

                    return new ImportResponse()
                    {
                        IsSuccess = true
                    };
                default:
                    return null;
            }
        }

        private async Task<object> GetScenarioLibrariesAndResourcesAsync(Guid scenarioId)
        {
            var libraries = await _moduleCustomLibraryRepository.GetsByAncestorAsync(scenarioId);
            var resources = await _moduleCustomResourceRepository.GetsByAncestorAsync(scenarioId);
            return new List<object>() { libraries, resources };
        }

        private async Task<IEnumerable<object>> GetLibrariesAndResourcesAsync(Guid moduleId)
        {
            var libraries = await _moduleCustomLibraryRepository.GetsAsync(moduleId);
            var resources = await _moduleCustomResourceRepository.GetsAsync(moduleId);
            return new List<object>() { libraries, resources };
        }

        private async Task AddLibrariesAsync(Guid moduleId, IEnumerable<ModuleCustomLibraryInfo> libraries)
        {
            foreach (var library in libraries)
            {
                library.ModuleId = moduleId;
                await _moduleCustomLibraryRepository.AddAsync(library);
            }
        }

        private async Task AddResourcesAsync(Guid moduleId, IEnumerable<ModuleCustomResourceInfo> resources)
        {
            foreach (var resource in resources)
            {
                resource.ModuleId = moduleId;
                await _moduleCustomResourceRepository.AddAsync(resource);
            }
        }

        #endregion
    }
}
