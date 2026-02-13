using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetNuke.Common.Utilities;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Data.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Module;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Export;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Import;
using NitroSystem.Dnn.BusinessEngine.Core.Security;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataService.Module
{
    public class ModuleLibraryAndResourceService : IModuleLibraryAndResourceService, IExportable, IImportable
    {
        private readonly ICacheService _cacheService;
        private readonly IRepositoryBase _repository;

        public ModuleLibraryAndResourceService(ICacheService cacheService, IRepositoryBase repository)
        {
            _cacheService = cacheService;
            _repository = repository;
        }

        public async Task<IEnumerable<ModuleCustomLibraryViewModel>> GetModuleCustomLibrariesAsync(Guid moduleId)
        {
            var libraries = await _repository.GetByScopeAsync<ModuleCustomLibraryView>(moduleId, "LoadOrder");
            var resources = await _repository.GetByScopeAsync<ModuleCustomLibraryResourceView>(moduleId);

            return HybridMapper.MapWithChildren<ModuleCustomLibraryView, ModuleCustomLibraryViewModel,
                                                    ModuleCustomLibraryResourceView, ModuleCustomLibraryResourceViewModel>(
                parents: libraries,
                children: resources,
                parentKeySelector: p => p.Id,
                childKeySelector: c => c.LibraryId,
                assignChildren: (parent, childs) => parent.Resources = childs
            );
        }

        public async Task<IEnumerable<ModuleCustomResourceViewModel>> GetModuleCustomResourcesAsync(Guid moduleId)
        {
            var resources = await _repository.GetByScopeAsync<ModuleCustomResourceInfo>(moduleId, "LoadOrder");

            return HybridMapper.MapCollection<ModuleCustomResourceInfo, ModuleCustomResourceViewModel>(resources);
        }

        public async Task<Guid> SaveModuleCustomLibraryAsync(ModuleCustomLibraryViewModel library, bool isNew)
        {
            var objModuleCustomLibraryInfo = HybridMapper.Map<ModuleCustomLibraryViewModel, ModuleCustomLibraryInfo>(library);

            if (isNew)
                objModuleCustomLibraryInfo.Id = await _repository.AddAsync<ModuleCustomLibraryInfo>(objModuleCustomLibraryInfo);
            else
            {
                var isUpdated = await _repository.UpdateAsync<ModuleCustomLibraryInfo>(objModuleCustomLibraryInfo);
                if (!isUpdated) ErrorHandling.ThrowUpdateFailedException(objModuleCustomLibraryInfo);
            }
            return objModuleCustomLibraryInfo.Id;
        }

        public async Task<Guid> SaveModuleCustomResourceAsync(ModuleCustomResourceViewModel resource, bool isNew)
        {
            var objModuleCustomResourceInfo = HybridMapper.Map<ModuleCustomResourceViewModel, ModuleCustomResourceInfo>(resource);

            if (isNew)
                objModuleCustomResourceInfo.Id = await _repository.AddAsync<ModuleCustomResourceInfo>(objModuleCustomResourceInfo);
            else
            {
                var isUpdated = await _repository.UpdateAsync<ModuleCustomResourceInfo>(objModuleCustomResourceInfo);
                if (!isUpdated) ErrorHandling.ThrowUpdateFailedException(objModuleCustomResourceInfo);
            }

            return objModuleCustomResourceInfo.Id;
        }

        public async Task SortModuleCustomLibraries(LibraryOrResource target, IEnumerable<SortInfo> items)
        {
            if (target == LibraryOrResource.Library)
                await _repository.ExecuteStoredProcedureAsync("dbo.BusinessEngine_Studio_SortModuleCustomLibraries",
                    new { JsonData = items.ToJson() });
            else if (target == LibraryOrResource.Resource)
                await _repository.ExecuteStoredProcedureAsync("dbo.BusinessEngine_Studio_SortModuleCustomResources",
                    new { JsonData = items.ToJson() });

            _cacheService.ClearByPrefix("BE_ModuleCustomLibraries_");
            _cacheService.ClearByPrefix("BE_ModuleCustomResources_");
        }

        public async Task<bool> DeleteModuleCustomLibraryAsync(Guid moduleId)
        {
            return await _repository.DeleteAsync<ModuleCustomLibraryInfo>(moduleId);
        }

        public async Task<bool> DeleteModuleCustomResourceAsync(Guid moduleId)
        {
            return await _repository.DeleteAsync<ModuleCustomResourceInfo>(moduleId);
        }

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
                        await AddLibrariesAsync(moduleId, (IEnumerable<ModuleCustomLibraryInfo>)items[0]);

                    if (items[1] as IEnumerable<ModuleCustomResourceInfo> != null)
                        await AddResourcesAsync(moduleId, (IEnumerable<ModuleCustomResourceInfo>)items[1]);

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
            var libraries = new List<ModuleCustomLibraryInfo>();
            var resources = new List<ModuleCustomResourceInfo>();

            var moduleIds = await _repository.GetColumnsValueAsync<ModuleInfo, Guid>("Id", "ScenarioId", scenarioId);
            foreach (var moduleId in moduleIds)
            {
                libraries.AddRange(await _repository.GetByScopeAsync<ModuleCustomLibraryInfo>(moduleId));
                resources.AddRange(await _repository.GetByScopeAsync<ModuleCustomResourceInfo>(moduleId));
            }

            return new List<object>() { libraries, resources };
        }

        private async Task<IEnumerable<object>> GetLibrariesAndResourcesAsync(Guid moduleId)
        {
            var libraries = await _repository.GetByScopeAsync<ModuleCustomLibraryInfo>(moduleId);
            var resources = await _repository.GetByScopeAsync<ModuleCustomResourceInfo>(moduleId);

            return new List<object>() { libraries, resources };
        }

        private async Task AddLibrariesAsync(Guid moduleId, IEnumerable<ModuleCustomLibraryInfo> libraries)
        {
            foreach (var library in libraries)
            {
                library.ModuleId = moduleId;

                await _repository.AddAsync<ModuleCustomLibraryInfo>(library);
            }
        }

        private async Task AddResourcesAsync(Guid moduleId, IEnumerable<ModuleCustomResourceInfo> resources)
        {
            foreach (var resource in resources)
            {
                resource.ModuleId = moduleId;

                await _repository.AddAsync<ModuleCustomResourceInfo>(resource);
            }
        }

        #endregion
    }
}
