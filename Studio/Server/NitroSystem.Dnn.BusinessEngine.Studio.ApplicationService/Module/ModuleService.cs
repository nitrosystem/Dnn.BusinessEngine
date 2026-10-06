using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Template;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Views.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Procedures.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Core.BackgroundJob;
using NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.BackgroundJobs;

namespace NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.Module
{
    public class ModuleService : IModuleService
    {
        private readonly IBaseService _baseService;
        private readonly IModuleRepository _moduleRepository;
        private readonly IModuleEventTypeRepository _moduleEventTypeRepository;
        private readonly IModuleOutputResourceRepository _moduleOutputResourceRepository;
        private readonly ITemplateRepository _templateRepository;
        private readonly BackgroundJobWorker _backgroundJobWorker;

        public ModuleService(
            IBaseService baseService,
            IModuleRepository moduleRepository,
            IModuleEventTypeRepository moduleEventTypeRepository,
            IModuleOutputResourceRepository moduleOutputResourceRepository,
            ITemplateRepository templateRepository,
            BackgroundJobWorker backgroundJobWorker)
        {
            _baseService = baseService;
            _moduleRepository = moduleRepository;
            _moduleEventTypeRepository = moduleEventTypeRepository;
            _moduleOutputResourceRepository = moduleOutputResourceRepository;
            _templateRepository = templateRepository;
            _backgroundJobWorker = backgroundJobWorker;

        }

        #region Module 

        public async Task<string> GetModuleNameAsync(Guid moduleId)
        {
            return await _moduleRepository.GetModuleNameAsync(moduleId);
        }

        public async Task<ModuleViewModel> GetModuleViewModelAsync(Guid moduleId, string basePath = "")
        {
            var module = await _moduleRepository.GetViewAsync(moduleId);
            return await HybridMapper.MapAsync<ModuleView, ModuleViewModel>(module,
                async (src, dest) =>
                {
                    if (!string.IsNullOrEmpty(basePath))
                    {
                        var modulePath = await GetModulePathAsync(src.ScenarioId, src.Id, src.ParentId, src.ModuleName, basePath);
                        dest.LayoutTemplate = await FileUtil.GetFileContentAsync($@"{modulePath}\_layout.html");
                        dest.LayoutCss = await FileUtil.GetFileContentAsync($@"{modulePath}\_layout.css");
                    }
                });
        }

        public async Task<IEnumerable<ModuleViewModel>> GetModulesViewModelAsync(Guid scenarioId)
        {
            var modules = await _moduleRepository.GetsViewAsync(scenarioId);
            return HybridMapper.MapCollection<ModuleView, ModuleViewModel>(modules);
        }

        public async Task<string> GetModulePathAsync(
           Guid scenarioId,
           Guid moduleId,
           Guid? parentModuleId,
           string moduleName,
           string basePath)
        {
            var scenarioName = await _baseService.GetScenarioNameAsync(scenarioId);
            var parentModuleName = parentModuleId.HasValue
                        ? await GetModuleNameAsync(parentModuleId.Value)
                        : string.Empty;
            return GetModulePath(scenarioName, parentModuleName, moduleName, basePath);
        }

        public string GetModulePath(string scenarioName, string parentModuleName, string moduleName, string basePath)
        {
            var scenarioFolder = StringHelper.ToKebabCase(scenarioName);
            var parentFolder = !string.IsNullOrEmpty(parentModuleName)
                ? StringHelper.ToKebabCase(parentModuleName) + @"\"
                : string.Empty;
            var moduleFolder = StringHelper.ToKebabCase(moduleName);
            return $@"{basePath}\business-engine\{scenarioFolder}\{parentFolder}{moduleFolder}\";
        }

        public async Task<Guid> CreateModuleAsync(ModuleViewModel module, int userId, string basePath)
        {
            var objModuleInfo = HybridMapper.Map<ModuleViewModel, ModuleInfo>(module);
            if (objModuleInfo.Id == Guid.Empty)
            {
                objModuleInfo.Id = await _moduleRepository.AddAsync(objModuleInfo);
            }
            else
            {
                var oldModule = await _moduleRepository.GetViewAsync(module.Id);

                await _moduleRepository.UpdateAsync(objModuleInfo);

                if (oldModule?.ModuleName != module.ModuleName)
                {
                    RenameModuleFolder(basePath, oldModule.ModuleName, module.ModuleName, oldModule.ParentModuleName, oldModule.ScenarioName);
                    await EnqueueRebuildIfRequiredAsync(module.ScenarioId, module.Id, userId, basePath);
                }
            }

            return objModuleInfo.Id;
        }

        public async Task<bool> DeleteModuleAsync(Guid moduleId)
        {
            return await _moduleRepository.DeleteAsync(moduleId);
        }

        public async Task EnqueueRebuildIfRequiredAsync(Guid scenarioId, Guid moduleId, int userId, string basePath)
        {
            if (await IsRebuildRequiredAsync(moduleId))
            {
                var scenarioName = await _baseService.GetScenarioNameAsync(scenarioId);

                var job = new JobContext { JobType = typeof(BuildModuleJob) };
                job.Set<Guid>("ModuleId", moduleId);
                job.Set<string>("BasePath", basePath);
                job.Set<int>("UserId", userId);
                _backgroundJobWorker.Enqueue(job);
            }
        }

        public void RenameModuleFolder(string basePath, string oldModuleName, string newModuleName, string parentModuleName, string scenarioName)
        {
            string oldModuleFolder = StringHelper.ToKebabCase(oldModuleName);
            string newModuleFolder = StringHelper.ToKebabCase(newModuleName);
            string parentModuleFolder = !string.IsNullOrEmpty(parentModuleName)
                ? StringHelper.ToKebabCase(newModuleName) + @"\"
                : string.Empty;
            string oldModulePath = $@"{basePath}{oldModuleName}\{parentModuleName}{oldModuleName}";
            string newModulePath = $@"{basePath}{oldModuleName}\{parentModuleName}{newModuleName}";

            FileUtil.RenameFolder(oldModulePath, newModulePath);
        }

        #endregion

        #region Building Module

        public async Task<ModuleDto> GetDataForModuleBuildingAsync(Guid moduleId)
        {
            var module = await _moduleRepository.GetViewAsync(moduleId);
            var data = await _moduleRepository.GetDataForBuildAsync(moduleId);
            var parentModuleName = module.ParentId.HasValue
                ? await _moduleRepository.GetModuleNameAsync(module.ParentId.Value)
                : string.Empty;

            var builder = new CollectionMappingBuilder<ModuleView, ModuleDto>();

            builder.AddChildAsync<ModuleFieldSpResult, ModuleFieldDto, Guid>(
              source: data.Fields,
              parentKey: parent => parent.Id,
              childKey: child => child.ModuleId,
              assign: (dest, children) => dest.Fields = children,
              configAction: async (src, dest) =>
               {
                   if (src.HasDataSource)
                       dest.DataSource = HybridMapper.Map<ModuleFieldDataSourceSpResult, ModuleFieldDataSourceDto>(
                               data.FieldsDataSource.FirstOrDefault(d => d.FieldId == src.Id) ?? new ModuleFieldDataSourceSpResult());

                   var dict = data.FieldsSettings.GroupBy(c => c.FieldId).ToDictionary(g => g.Key, g => g.AsEnumerable());
                   if (dict.TryGetValue(src.Id, out var settings))
                       dest.Settings = settings.ToDictionary(x => x.SettingName, x => CastingHelper.ConvertStringToObject(x.SettingValue));

                   dest.GlobalSettings = ReflectionUtil.ConvertDictionaryToObject<ModuleFieldGlobalSettings>(dest.Settings) ?? new ModuleFieldGlobalSettings();

                   await Task.CompletedTask;
               }
            );

            builder.AddChildAsync<ModuleResourceSpResult, ModuleResourceDto, Guid>(
               source: data.Resources,
               parentKey: parent => parent.Id,
               childKey: child => child.ModuleId,
               assign: (dest, children) => dest.Resources = children
            );

            builder.AddChildAsync<ModuleResourceSpResult, ModuleResourceDto, Guid>(
              source: data.ExternalResources,
              parentKey: parent => parent.Id,
              childKey: child => child.ModuleId,
              assign: (dest, children) => dest.ExternalResources = children
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

        public async Task<bool> IsRebuildRequiredAsync(Guid moduleId)
        {
            return await _moduleRepository.IsRebuildRequiredAsync(moduleId);
        }

        public async Task DeleteModuleResourcesAsync(Guid moduleId)
        {
            await _moduleOutputResourceRepository.DeleteAsync(moduleId);
        }

        public async Task BulkInsertModuleOutputResourcesAsync(int? sitePageId, IEnumerable<ModuleResourceDto> resources)
        {
            var outputResources = HybridMapper.MapCollection<ModuleResourceDto, ModuleOutputResourceInfo>(resources,
                (src, dest) => dest.SitePageId = sitePageId);

            await _moduleOutputResourceRepository.BulkInsertAsync(outputResources);
        }

        #endregion

        #region Module Template

        public async Task<ModuleTemplateViewModel> GetTemplateViewModelAsync(Guid moduleId)
        {
            var module = await _moduleRepository.GetAsync(moduleId);
            return HybridMapper.Map<ModuleInfo, ModuleTemplateViewModel>(module);
        }

        public async Task<Guid?> GetTemplateIdAsync(Guid moduleId)
        {
            var module = await _moduleRepository.GetAsync(moduleId);
            var templates = await _templateRepository.GetsAsync();
            return templates.FirstOrDefault(t => t.ModuleType.HasValue && t.ModuleType.Value == module.ModuleType &&
                                                        t.TemplateName == module.Template)?.Id;
        }

        public async Task UpdateTemplateAsync(ModuleTemplateViewModel module)
        {
            var objModuleInfo = HybridMapper.Map<ModuleTemplateViewModel, ModuleInfo>(module);
            await _moduleRepository.UpdateAsync(objModuleInfo, "Template", "Theme", "ThemeCssClass", "PreloadingTemplate", "LayoutTemplate", "LayoutCss");
        }

        #endregion

        #region Module Event Type

        public async Task<List<ModuleEventTypeListItem>> GetModuleEventTypesListItem(string fieldType = null)
        {
            var events = await _moduleEventTypeRepository.GetsAsync("ViewOrder");
            var component = string.IsNullOrEmpty(fieldType)
                ? "Module"
                : fieldType;

            var result = HybridMapper.MapCollection<ModuleEventTypeInfo, ModuleEventTypeListItem>(events.Where(e => e.Component == component)).ToList();
            if (result.Count > 0)
                result.Add(new ModuleEventTypeListItem()
                {
                    EventName = "OnActionCompleted",
                    Title = "On Action Completed"
                });

            return result;
        }

        #endregion
    }
}