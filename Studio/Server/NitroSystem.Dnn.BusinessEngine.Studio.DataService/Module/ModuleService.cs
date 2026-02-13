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
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Models;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ListItems;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Export;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Import;
using Newtonsoft.Json.Linq;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataService.Module
{
    public class ModuleService : IModuleService, IExportable, IImportable
    {
        private readonly IRepositoryBase _repository;

        public ModuleService(IServiceProvider serviceProvider, IRepositoryBase repository)
        {
            _repository = repository;
        }

        #region Module Services

        public async Task<IEnumerable<ModuleViewModel>> GetModulesViewModelAsync(Guid scenarioId)
        {
            var modules = await _repository.GetByScopeAsync<ModuleView>(scenarioId);

            return HybridMapper.MapCollection<ModuleView, ModuleViewModel>(modules);
        }

        public async Task<List<ModuleEventTypeListItem>> GetModuleEventTypes(string fieldType = null)
        {
            var component = string.IsNullOrEmpty(fieldType)
                ? "Module"
                : fieldType;

            var events = await _repository.GetAllAsync<ModuleEventTypeInfo>("ViewOrder");

            var result = HybridMapper.MapCollection<ModuleEventTypeInfo, ModuleEventTypeListItem>(events.Where(e => e.Component == component)).ToList();
            if (result.Count > 0)
                result.Add(new ModuleEventTypeListItem()
                {
                    EventName = "OnActionCompleted",
                    Title = "On Action Completed"
                });

            return result;
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
                objModuleInfo.Id = await _repository.AddAsync<ModuleInfo>(objModuleInfo);
            else
            {
                var isUpdated = await _repository.UpdateAsync<ModuleInfo>(objModuleInfo);
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
            var data = await _repository.ExecuteStoredProcedureMultipleAsync<ModuleFieldSpResult, ModuleFieldDataSourceSpResult,
                ModuleFieldSettingSpResult, ModuleResourceSpResult, ModuleResourceSpResult>(
                "dbo.BusinessEngine_Studio_GetModuleDataForBuild", "BE_Modules_Fields_Settings_Studio_GetModuleDataForBuild_",
                new
                {
                    ModuleId = moduleId
                }
            );

            var fields = data.Item1;
            var fieldsDataSource = data.Item2;
            var fieldsSettings = data.Item3;
            var resources = data.Item4;
            var externalResources = data.Item5;
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
                   if (src.HasDataSource)
                       dest.DataSource = HybridMapper.Map<ModuleFieldDataSourceSpResult, ModuleFieldDataSourceDto>(
                               fieldsDataSource.FirstOrDefault(d => d.FieldId == src.Id) ?? new ModuleFieldDataSourceSpResult());

                   var dict = fieldsSettings.GroupBy(c => c.FieldId).ToDictionary(g => g.Key, g => g.AsEnumerable());
                   if (dict.TryGetValue(src.Id, out var settings))
                       dest.Settings = settings.ToDictionary(x => x.SettingName, x => CastingHelper.ConvertStringToObject(x.SettingValue));

                   dest.GlobalSettings = ReflectionUtil.ConvertDictionaryToObject<ModuleFieldGlobalSettings>(dest.Settings) ?? new ModuleFieldGlobalSettings();

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

        #region Import Export

        public async Task<ExportResponse> ExportAsync(ExportContext context)
        {
            switch (context.Scope)
            {
                case ImportExportScope.ScenarioFullComponents:
                    var modules = await GetModulesAsync(context.Get<Guid>("ScenarioId"));

                    return new ExportResponse()
                    {
                        Result = modules,
                        IsSuccess = true
                    };
                case ImportExportScope.Module:
                    var moduleId = context.Get<Guid>("ModuleId");
                    var module = await GetModuleAsync(moduleId);

                    return new ExportResponse()
                    {
                        Result = module,
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
                case ImportExportScope.ScenarioFullComponents:
                    var modules = JsonConvert.DeserializeObject<IEnumerable<ModuleInfo>>(json);

                    context.TryGet<JObject>("ModulesNewPages", out var newPages);

                    foreach (var mod in modules.OrderBy(m => m.ParentId))
                    {
                        if (mod.SiteModuleId.HasValue)
                        {
                            var portalId = context.Get<int>("CurrentPortalId");
                            var userId = context.Get<int>("CurrentUserId");
                            var newTabId = newPages[mod.Id.ToString()].Value<int>();
                            var moduleDefName = mod.ModuleType == 0
                                ? "BusinessEngine.Dashboard"
                                : "BusinessEngine.Module";

                            mod.SiteModuleId = await AddModuleToSitePage(portalId, userId, newTabId, moduleDefName, mod.ModuleTitle);
                        }

                        await SaveModuleAsync(mod);
                    }

                    return new ImportResponse()
                    {
                        IsSuccess = true
                    };
                case ImportExportScope.Module:
                    var module = JsonConvert.DeserializeObject<ModuleInfo>(json);

                    var oldScenarioId = module.ScenarioId;
                   
                    var scenarioId = context.Get<Guid>("ScenarioId");
                    var moduleName = context.Get<string>("ModuleName");
                    var moduleTitle = context.Get<string>("ModuleTitle");

                    context.TryGet<Guid?>("ModuleId", out var moduleId);
                    context.TryGet<Guid?>("ParentId", out var parentId);
                    context.TryGet<int?>("SiteModuleId", out var siteModuleId);
                    context.TryGet<bool?>("DeleteModuleBeforeImport", out var deleteModule);

                    module.ScenarioId = scenarioId;
                    module.ModuleName = moduleName;
                    module.ModuleTitle = moduleTitle;
                    module.ParentId = parentId;
                    module.SiteModuleId = siteModuleId;

                    var id = await SaveModuleAsync(module, moduleId, deleteModule);
                    
                    context.DataTrack.Add("ScenarioId", scenarioId);
                    context.DataTrack.Add("ModuleId", id);
                    context.DataTrack.Add("OldScenarioId", oldScenarioId);

                    return new ImportResponse()
                    {
                        Result = id,
                        IsSuccess = true
                    };

                default:
                    return null;
            }
        }

        private async Task<int> AddModuleToSitePage(int portalId, int userId, int pageId, string moduleDefName, string moduleTitle, string paneName = "ContentPane")
        {
            return await _repository.ExecuteStoredProcedureAsync<int>("dbo.BusinessEngine_AddModuleToSitePage", "",
                new
                {
                    Portald = portalId,
                    UserId = userId,
                    PageId = pageId,
                    ModuleDefName = moduleDefName,
                    ModuleTitle = moduleTitle,
                    PaneName = paneName
                }
            );
        }

        private async Task<IEnumerable<ModuleInfo>> GetModulesAsync(Guid scenarioId)
        {
            return await _repository.GetByScopeAsync<ModuleInfo>(scenarioId);
        }

        private async Task<ModuleInfo> GetModuleAsync(Guid moduleId)
        {
            return await _repository.GetAsync<ModuleInfo>(moduleId);
        }

        private async Task<Guid> SaveModuleAsync(ModuleInfo module, Guid? moduleId, bool? deleteModule)
        {
            if (moduleId.HasValue && deleteModule.HasValue && deleteModule.Value)
                await DeleteModuleAsync(moduleId.Value);
            else if (moduleId.HasValue && (!deleteModule.HasValue || !deleteModule.Value))
            {
                module.Id = moduleId.Value;
                await _repository.UpdateAsync<ModuleInfo>(module);

                return module.Id;
            }

            module.Id = Guid.NewGuid();
            return await _repository.AddAsync<ModuleInfo>(module);
        }

        private async Task SaveModuleAsync(ModuleInfo module)
        {
            await _repository.AddAsync<ModuleInfo>(module);
        }

        #endregion
    }
}