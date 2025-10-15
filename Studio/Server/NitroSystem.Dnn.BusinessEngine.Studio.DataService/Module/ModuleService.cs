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
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataService.Module
{
    public class ModuleService : IModuleService
    {
        private readonly IRepositoryBase _repository;

        public ModuleService(IUnitOfWork unitOfWork, ICacheService cacheService, IRepositoryBase repository)
        {
            _repository = repository;
        }

        #region Module Services

        public async Task<ModuleViewModel> GetModuleViewModelAsync(Guid moduleId)
        {
            var module = await _repository.GetAsync<ModuleView>(moduleId);

            return HybridMapper.Map<ModuleView, ModuleViewModel>(module);
        }

        public async Task<IEnumerable<ModuleViewModel>> GetModulesViewModelAsync(Guid scenarioId)
        {
            var modules = await _repository.GetByScopeAsync<ModuleView>(scenarioId);

            return HybridMapper.MapCollection<ModuleView, ModuleViewModel>(modules);
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

        public async Task<bool?> IsValidModuleName(Guid scenarioId, Guid? moduleId, string moduleName)
        {
            return await _repository.ExecuteStoredProcedureScalerAsync<bool?>(
                "dbo.BusinessEngine_Studio_IsValidModuleName", "BE_Modules_IsValidModuleName_" + moduleName,
                new
                {
                    ScenarioId = scenarioId,
                    ModuleId = moduleId,
                    ModuleName = moduleName
                });
        }

        public async Task<bool> DeleteModuleAsync(Guid moduleId)
        {
            return await _repository.DeleteAsync<ModuleInfo>(moduleId);
        }

        #endregion

        #region Module Template Services

        public async Task<ModuleTemplateViewModel> GetModuleTemplateViewModelAsync(Guid moduleId)
        {
            var module = await _repository.GetAsync<ModuleInfo>(moduleId);

            return HybridMapper.Map<ModuleInfo, ModuleTemplateViewModel>(module);
        }

        public async Task<bool> UpdateModuleTemplateAsync(ModuleTemplateViewModel module)
        {
            var objModuleInfo = HybridMapper.Map<ModuleTemplateViewModel, ModuleInfo>(module);

            return await _repository.UpdateAsync(
                objModuleInfo,
                "Template",
                "Theme",
                "PreloadingTemplate",
                "LayoutTemplate",
                "LayoutCss"
            );
        }

        #endregion

        #region Building Module

        public async Task<ModuleDto> GetDataForModuleBuildingAsync(Guid moduleId)
        {
            var module = await _repository.GetAsync<ModuleView>(moduleId);
            var data = await _repository.ExecuteStoredProcedureMultipleAsync<ModuleFieldSpResult, ModuleFieldSettingSpResult,
                    ModuleResourceSpResult, ModuleResourceSpResult>(
                "dbo.BusinessEngine_Studio_GetModuleDataForBuild", "BE_Modules_Fields_Settings_Build_" + moduleId,
                new
                {
                    ModuleId = moduleId
                }
            );

            var fields = data.Item1;
            var fieldsSettings = data.Item2;
            var resources = data.Item3;
            var externalResources = data.Item4;

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

            var result = await builder.BuildAsync(module);
            return result;
        }

        public async Task<bool> DeleteModuleResourcesAsync(Guid moduleId)
        {
            return await _repository.DeleteByScopeAsync<ModuleOutputResourceInfo>(moduleId);
        }

        public async Task BulkInsertModuleOutputResources(int? sitePageId, IEnumerable<ModuleResourceDto> resources)
        {
            var outputResources = HybridMapper.MapCollection<ModuleResourceDto, ModuleOutputResourceInfo>(resources,
                (src, dest) => dest.SitePageId = sitePageId);

            try
            {
                await _repository.BulkInsertAsync<ModuleOutputResourceInfo>(outputResources);
            }
            catch (Exception ex)
            {
            }
        }

        #endregion
    }
}

//    using (var reader = await spResourceReaderTask)
//    {
//        while (reader.Read())
//        {
//            resourcesToBuild.Add(new BuildModuleResourceDto()
//            {
//                ModuleId = GlobalHelper.GetSafeGuid(reader["ModuleId"]),
//                ResourceType = (ResourceType)reader["ResourceType"],
//                ActionType = (ActionType)reader["ActionType"],
//                ResourcePath = GlobalHelper.GetSafeString(reader["ResourcePath"]),
//                EntryType = GlobalHelper.GetSafeString(reader["EntryType"]),
//                Additional = GlobalHelper.GetSafeString(reader["Additional"]),
//                CacheKey = GlobalHelper.GetSafeString(reader["CacheKey"]),
//                Condition = GlobalHelper.GetSafeString(reader["Condition"]),
//                LoadOrder = GlobalHelper.GetSafeInt(reader["LoadOrder"])
//            });
//        }
//    }

//#region Build Module Services

//public async Task BuildModuleAsync(Guid moduleId, PortalSettings portalSettings, HttpContext context)
//{
//    

//    var pageId = await spPageIdTask;
//    

//    var moduleToBuild = HybridMapper.Map<ModuleView, BuildModuleDto>(module);
//    var fieldsToBuild = HybridMapper.MapCollection<ModuleFieldInfo, BuildModuleFieldDto>(fields,
//        (src, dest) =>
//        {
//            var dict = fieldsSettings.GroupBy(c => c.FieldId)
//                             .ToDictionary(g => g.Key, g => g.AsEnumerable());

//            var items = dict.TryGetValue(src.Id, out var settings);
//            dest.Settings = settings.ToDictionary(x => x.SettingName, x => CastingHelper.ConvertStringToObject(x.SettingValue));

//            dest.GlobalSettings = ReflectionUtil.ConvertDictionaryToObject<ModuleFieldGlobalSettings>(dest.Settings);
//        });

//    var resourcesToBuild = new List<BuildModuleResourceDto>();
//    using (var reader = await spResourceReaderTask)
//    {
//        while (reader.Read())
//        {
//            resourcesToBuild.Add(new BuildModuleResourceDto()
//            {
//                ModuleId = GlobalHelper.GetSafeGuid(reader["ModuleId"]),
//                ResourceType = (ResourceType)reader["ResourceType"],
//                ActionType = (ActionType)reader["ActionType"],
//                ResourcePath = GlobalHelper.GetSafeString(reader["ResourcePath"]),
//                EntryType = GlobalHelper.GetSafeString(reader["EntryType"]),
//                Additional = GlobalHelper.GetSafeString(reader["Additional"]),
//                CacheKey = GlobalHelper.GetSafeString(reader["CacheKey"]),
//                Condition = GlobalHelper.GetSafeString(reader["Condition"]),
//                LoadOrder = GlobalHelper.GetSafeInt(reader["LoadOrder"])
//            });
//        }
//    }

//    var status = await _buildModule.PrepareBuild(moduleToBuild, _repository, portalSettings);
//    if (status.IsReadyToBuild)
//    {
//        var pageResources = await _repository.GetItemsByColumnAsync<PageResourceInfo>("SitePageId", pageId);
//        var pageResourcesDto = HybridMapper.MapCollection<PageResourceInfo, PageResourceDto>(pageResources);

//        var finalResources = await _buildModule.ExecuteBuildAsync(moduleToBuild, fieldsToBuild, resourcesToBuild, pageId, portalSettings, context);
//        var mappedResources = HybridMapper.MapCollection<PageResourceDto, PageResourceInfo>(finalResources);
//        await _repository.BulkInsertAsync<PageResourceInfo>(mappedResources);

//        _cacheService.ClearByPrefix("BE_Modules_");
//    }
//    else if (status.ExceptionError != null)
//        throw status.ExceptionError ?? throw new Exception("The module is not ready to be build!");
//}

//#endregion