using System;
using System.Web;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using DotNetNuke.Entities.Portals;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModuleEngine.Enums;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModuleEngine.Models;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModuleEngine.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModuleEngine.Dto;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModuleEngine
{
    public class BuildModule : IBuildModule
    {
        IBuildModuleLayout _buildModuleLayout;
        IResourceMachine _resourceMachine;
        IModuleBuildLockService _lockService;

        private readonly List<MachineResourceInfo> _machineResources;

        public BuildModule(
            IBuildModuleLayout buildModuleLayout,
            IResourceMachine resourceMachine,
            IModuleBuildLockService lockService)
        {
            _buildModuleLayout = buildModuleLayout;
            _resourceMachine = resourceMachine;
            _lockService = lockService;

            _machineResources = new List<MachineResourceInfo>();
        }

        public async Task<PrebuildResultDto> PrepareBuild(BuildModuleDto module, IRepositoryBase repository, PortalSettings portalSettings)
        {
            var result = new PrebuildResultDto();

            var lockAcquired = await _lockService.TryLockAsync(module.Id, 1000); // Wait up to 1 second.
            if (!lockAcquired)
            {
                throw new InvalidOperationException("This module is currently being build. Please try again in a few moments..");
            }

            await repository.DeleteByScopeAsync<PageResourceInfo>(module.Id);

            try
            {
                result.TemplateDirectoryPath = $@"{module.BuildPath}\"
                        .Replace("[BUILDPATH]", portalSettings.HomeSystemDirectoryMapPath + @"business-engine\");

                var deleteResult = FileUtil.DeleteDirectory(result.TemplateDirectoryPath);

                result.IsDeletedOldFiles = result.IsReadyToBuild = deleteResult.isDeleted;
                //if (!result.IsDeletedOldFiles) result.ExceptionError = deleteResult.errorException;
            }
            finally
            {
                _lockService.ReleaseLock(module.Id);
            }

            return result;
        }

        public async Task<IEnumerable<PageResourceDto>> ExecuteBuildAsync(
                BuildModuleDto module,
                IEnumerable<BuildModuleFieldDto> fields,
                IEnumerable<BuildModuleResourceDto> resources,
                int pageId,
                PortalSettings portalSettings,
                HttpContext context)
        {
            var dict = resources.GroupBy(r => r.ModuleId)
                                              .ToDictionary(r => r.Key, r => r.AsEnumerable());
            foreach (var moduleId in dict.Keys)
            {
                dict.TryGetValue(moduleId, out var items);

                var resourceType = items.GroupBy(r => r.ActionType)
                                        .ToDictionary(
                                            r => r.Key,
                                            r => r.AsEnumerable()
                                        );

                ProcessGetResources(module, resourceType[ActionType.GetResourcePath]);

                ProcessLoadResources(module, resourceType[ActionType.LoadResourceContent], fields, portalSettings);
            }

            var moduleResources = await _resourceMachine.RunAsync(_machineResources, context);

            var result = HybridMapper.MapCollection<ModuleResourceInfo, PageResourceDto>(moduleResources).ToList();
            result.ForEach(r => { r.DnnPageId = pageId; r.IsActive = true; });
            return result;
        }

        private void ProcessGetResources(BuildModuleDto module, IEnumerable<BuildModuleResourceDto> resources)
        {
            var dict = resources.GroupBy(r => r.ResourceType)
                   .ToDictionary(
                       r => r.Key,
                       r => r.AsEnumerable()
                   );

            foreach (ResourceType resourceType in Enum.GetValues(typeof(ResourceType)))
            {
                if (dict.TryGetValue(resourceType, out var items))
                {
                    var firstItem = items.First();

                    _machineResources.Add(
                        new MachineResourceInfo()
                        {
                            ModuleId = module.Id,
                            ActionType = ActionType.GetResourcePath,
                            AddToResources = true,
                            ResourceFiles = items.Select(rf =>
                                 new MachineResourceFileInfo()
                                 {
                                     ResourcePath = rf.ResourcePath,
                                     EntryType = rf.EntryType,
                                     Additional = rf.Additional,
                                     ContinueOnError = true
                                 }).ToList(),
                            Condition = firstItem.Condition,
                            OperationType = OperationType.AddResourcePathToModuleResources,
                            ContinueOnError = true,
                            Order = firstItem.LoadOrder
                        }
                    );
                }
            }
        }

        private void ProcessLoadResources(
            BuildModuleDto module,
            IEnumerable<BuildModuleResourceDto> resources,
            IEnumerable<BuildModuleFieldDto> fields,
            PortalSettings portalSettings)
        {
            var dict = resources
                .GroupBy(r => r.ResourceType)
                .ToDictionary(r => r.Key, r => r.AsEnumerable());

            foreach (var item in dict)
            {
                var items = dict[item.Key];
                if (items != null && items.Any())
                {
                    _machineResources.Add(
                           new MachineResourceInfo()
                           {
                               ModuleId = module.Id,
                               ActionType = ActionType.LoadResourceContent,
                               AddToResources = true,
                               ResourceFiles = HybridMapper.MapCollection<BuildModuleResourceDto, MachineResourceFileInfo>(items).ToList(),
                               Condition = items.First().Condition,
                               OperationType = OperationType.MergeResourceFiles,
                               MergeStrategy = ProccessMergeStrategy(item.Key, module, items, fields, portalSettings),
                               ContinueOnError = true,
                               Order = _machineResources.Count + 1
                           }
                       );
                }
            }
        }

        private ResourceMergeStrategy ProccessMergeStrategy(
            ResourceType resourceType,
            BuildModuleDto module,
            IEnumerable<BuildModuleResourceDto> resources,
            IEnumerable<BuildModuleFieldDto> fields,
            PortalSettings portalSettings)
        {
            ResourceMergeStrategy strategy = null;

            switch (resourceType)
            {
                case ResourceType.ModuleCss:
                    strategy = new ResourceMergeStrategy()
                    {
                        MergedCallback = args =>
                        {
                            return module.LayoutCss;
                        },
                        IgnoreLoadingResourceFiles = true,
                        MergedOutputFilePath = $@"{module.BuildPath}\{GetModuleFilename(resourceType, module.ModuleName)}"
                       .Replace("[BUILDPATH]", portalSettings.HomeSystemDirectoryMapPath + @"business-engine\"),
                        MergedResourcePath = $@"{module.BuildPath}/{GetModuleFilename(resourceType, module.ModuleName)}"
                               .Replace("[BUILDPATH]", portalSettings.HomeSystemDirectory + @"business-engine/").Replace(@"\", "/")
                    };

                    break;
                case ResourceType.FieldTypeTemplate:
                    strategy = new ResourceMergeStrategy()
                    {
                        MergedCallback = args =>
                        {
                            var fieldTypes = args[0] as IDictionary<(string entryType, string entryAdditional), string>;
                            return _buildModuleLayout.BuildLayout(module.LayoutTemplate, fieldTypes, fields);
                        },
                        MergedOutputFilePath = $@"{module.BuildPath}\module--{module.ModuleName}.html"
                               .Replace("[BUILDPATH]", portalSettings.HomeSystemDirectoryMapPath + @"business-engine\")
                    };

                    break;
                default:
                    strategy = new ResourceMergeStrategy()
                    {
                        MergedOutputFilePath = $@"{module.BuildPath}\{GetModuleFilename(resourceType, module.ModuleName)}"
                       .Replace("[BUILDPATH]", portalSettings.HomeSystemDirectoryMapPath + @"business-engine\"),
                        MergedResourcePath = $@"{module.BuildPath}/{GetModuleFilename(resourceType, module.ModuleName)}"
                               .Replace("[BUILDPATH]", portalSettings.HomeSystemDirectory + @"business-engine/").Replace(@"\", "/")
                    };

                    break;
            }

            return strategy;
        }

        private object GetModuleFilename(ResourceType resourceType, string moduleName)
        {
            switch (resourceType)
            {
                case ResourceType.ModuleCss:
                    return $"module--{moduleName}.css";
                case ResourceType.ModuleActionScript:
                    return $"modules--{moduleName}-actions.js";
                case ResourceType.FieldTypeScript:
                    return $"module--{moduleName}-field-scripts.js";
                case ResourceType.FieldDirectiveScript:
                    return $"module--{moduleName}-field-directive.js";
                case ResourceType.FieldTypeTheme:
                    return $"module--{moduleName}-field-themes.css";
                default:
                    return string.Empty;
            }
        }
    }
}
