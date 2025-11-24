using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Data.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Core.Security;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Views;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Procedures;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Dto;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataService.Module
{
    public class ModuleService : IModuleService
    {
        private readonly IRepositoryBase _repository;

        public ModuleService(IRepositoryBase repository)
        {
            _repository = repository;
        }

        #region Module Services

        public async Task<IEnumerable<ModuleViewModel>> GetModulesViewModelAsync(Guid scenarioId)
        {
            var modules = await _repository.GetByScopeAsync<ModuleView>(scenarioId);

            return HybridMapper.MapCollection<ModuleView, ModuleViewModel>(modules);
        }

        public async Task<ModuleViewModel> GetModuleViewModelAsync(Guid moduleId)
        {
            var module = await _repository.GetAsync<ModuleView>(moduleId);

            return HybridMapper.Map<ModuleView, ModuleViewModel>(module);
        }

        public async Task<string> GetModuleNameAsync(Guid moduleId)
        {
            return await _repository.GetColumnValueAsync<ModuleInfo, string>(moduleId, "ModuleName");
        }

        public async Task<Guid> SaveModuleAsync(ModuleViewModel module, bool isNew)
        {
            var objModuleInfo = HybridMapper.Map<ModuleViewModel, ModuleInfo>(module);

            if (isNew)
                objModuleInfo.Id = await _repository.AddAsync(objModuleInfo);
            else
            {
                var isUpdated = await _repository.UpdateAsync(objModuleInfo);
                if (!isUpdated) ErrorHandling.ThrowUpdateFailedException(objModuleInfo);
            }

            return objModuleInfo.Id;
        }

        public async Task<bool?> IsValidModuleNameAsync(Guid scenarioId, Guid? moduleId, string moduleName)
        {
            return await _repository.ExecuteStoredProcedureScalerAsync<bool?>(
                "dbo.BusinessEngine_Studio_IsValidModuleName", "BE_Modules_Studio_IsValidModuleName_",
                new
                {
                    ScenarioId = scenarioId,
                    ModuleId = moduleId,
                    ModuleName = moduleName
                });
        }

        public async Task<bool> IsRebuildRequired(Guid moduleId, string moduleName)
        {
            return await _repository.ExecuteStoredProcedureScalerAsync<bool>(
               "dbo.BusinessEngine_Studio_IsRebuildRequired", "BE_Modules_Studio_IsRebuildRequired_",
               new
               {
                   ModuleId = moduleId,
                   ModuleName = moduleName
               });
        }

        public async Task<bool> DeleteModuleAsync(Guid moduleId)
        {
            return await _repository.DeleteAsync<ModuleInfo>(moduleId);
        }

        #endregion

        #region Building Module

        public async Task<ModuleDto> GetDataForModuleBuildingAsync(Guid moduleId)
        {
            var module = await _repository.GetAsync<ModuleView>(moduleId);
            var data = await _repository.ExecuteStoredProcedureMultipleAsync<ModuleFieldSpResult, ModuleFieldSettingSpResult,
                    ModuleResourceSpResult, ModuleResourceSpResult>(
                "dbo.BusinessEngine_Studio_GetModuleDataForBuild", "BE_Modules_Fields_Settings_Studio_GetModuleDataForBuild_",
                new
                {
                    ModuleId = moduleId
                }
            );

            var fields = data.Item1;
            var fieldsSettings = data.Item2;
            var resources = data.Item3;
            var externalResources = data.Item4;
            var parentModuleName = module.ParentId.HasValue
                ? await _repository.GetColumnValueAsync<ModuleInfo, string>(module.ParentId.Value, "ModuleName")
                : string.Empty;

            var builder = new CollectionMappingBuilder<ModuleView, ModuleDto>();

            builder.AddChildAsync<ModuleFieldSpResult, ModuleFieldDto, Guid>(
              source: fields,
              parentKey: parent => parent.Id,
              childKey: child => child.ModuleId,
              assign: (dest, children) => dest.Fields = children.ToList(),
              configAction: async (src, dest) =>
               {
                   var dict = fieldsSettings.GroupBy(c => c.FieldId).ToDictionary(g => g.Key, g => g.AsEnumerable());
                   if (dict.TryGetValue(src.Id, out var settings))
                       dest.Settings = settings.ToDictionary(x => x.SettingName, x => CastingHelper.ConvertStringToObject(x.SettingValue));

                   await Task.CompletedTask;
               }
            );

            builder.AddChildAsync<ModuleResourceSpResult, ModuleResourceDto, Guid>(
               source: resources,
               parentKey: parent => parent.Id,
               childKey: child => child.ModuleId,
               assign: (dest, children) => dest.Resources = children.ToList()
            );

            builder.AddChildAsync<ModuleResourceSpResult, ModuleResourceDto, Guid>(
              source: externalResources,
              parentKey: parent => parent.Id,
              childKey: child => child.ModuleId,
              assign: (dest, children) => dest.ExternalResources = children.ToList()
           );

            var result = await builder.BuildAsync(
                source: module,
                afterMap: (src, dest) =>
                {
                    if (src.ParentId.HasValue)
                        dest.ParentModuleName = parentModuleName;
                });

            return result;
        }

        public async Task DeleteModuleResourcesAsync(Guid moduleId)
        {
            await _repository.DeleteByScopeAsync<ModuleOutputResourceInfo>(moduleId);
        }

        public async Task BulkInsertModuleOutputResourcesAsync(int? sitePageId, IEnumerable<ModuleResourceDto> resources)
        {
            var outputResources = HybridMapper.MapCollection<ModuleResourceDto, ModuleOutputResourceInfo>(resources,
                (src, dest) => dest.SitePageId = sitePageId);

            await _repository.BulkInsertAsync<ModuleOutputResourceInfo>(outputResources);
        }

        #endregion
    }
}