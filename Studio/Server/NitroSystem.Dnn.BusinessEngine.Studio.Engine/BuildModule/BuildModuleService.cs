using DotNetNuke.Data;
using DotNetNuke.Entities.Portals;
using NitroSystem.Dnn.BusinessEngine.Common.IO;
using NitroSystem.Dnn.BusinessEngine.Common.Reflection;
using NitroSystem.Dnn.BusinessEngine.Core.Cashing;
using NitroSystem.Dnn.BusinessEngine.Core.Contract;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.ExpressionService.ConditionParser;
using NitroSystem.Dnn.BusinessEngine.Core.Reflection;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Repository;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Enums;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Models;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.Dto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule
{
    public class BuildModuleService : IBuildModuleService
    {
        IBuildModuleLayout _buildModuleLayout;
        IResourceMachine _resourceMachine;
        IModuleBuildLockService _lockService;

        private readonly List<MachineResourceInfo> _machineResources;

        public BuildModuleService(
            IBuildModuleLayout buildModuleLayout,
            IResourceMachine resourceMachine,
            IModuleBuildLockService lockService
        )
        {
            _buildModuleLayout = buildModuleLayout;
            _resourceMachine = resourceMachine;
            _lockService = lockService;

            _machineResources = new List<MachineResourceInfo>();
        }

        public async Task<PrebuildResultDto> PrepareBuild(BuildModuleDto moduleToBuild, IRepositoryBase repository, PortalSettings portalSettings)
        {
            var result = new PrebuildResultDto();

            var lockAcquired = await _lockService.TryLockAsync(moduleToBuild.Id, 1000); // Wait up to 3 seconds.

            if (!lockAcquired)
            {
                throw new InvalidOperationException("This module is currently being build. Please try again in a few moments..");
            }

            await repository.DeleteByScopeAsync<PageResourceInfo>(moduleToBuild.Id);

            try
            {
                result.TemplateDirectoryPath = $@"{moduleToBuild.BuildPath}\"
                                .Replace("[BUILDPATH]", portalSettings.HomeSystemDirectoryMapPath + @"business-engine\");

                var deleteResult = FileUtil.DeleteDirectory(result.TemplateDirectoryPath);

                result.IsDeletedOldFiles = result.IsReadyToBuild = deleteResult.isDeleted;
                if (!result.IsDeletedOldFiles) result.ExceptionError = deleteResult.errorException;
            }
            finally
            {
                _lockService.ReleaseLock(moduleToBuild.Id);
            }

            return result;
        }

        public async Task<IEnumerable<PageResourceDto>> ExecuteBuildAsync(int? pageId, (DashboardType DashboardType, string Skin, string SkinPath) dashboard, IEnumerable<BuildModuleDto> modulesToBuild, IEnumerable<BuildModuleFieldDto> fieldsToBuild, IEnumerable<BuildModuleResourceDto> resourcesToBuild, IEnumerable<PageResourceDto> oldReesources, PortalSettings portalSettings, HttpContext context)
        {
            var result = new List<MachineResourceInfo>();

            var dict = resourcesToBuild.GroupBy(r => r.ModuleId)
                                              .ToDictionary(r => r.Key, r => r.AsEnumerable());
            foreach (var moduleId in dict.Keys)
            {
                dict.TryGetValue(moduleId, out var items);

                var moduleToBuild = modulesToBuild.FirstOrDefault(m => m.Id == moduleId);
                var resourceType = items.GroupBy(r => r.ActionType)
                                        .ToDictionary(
                                            r => r.Key,
                                            r => r.AsEnumerable()
                                        );

                ProcessGetResources(moduleToBuild, resourceType[ActionType.GetResourcePath]);

                ProcessLoadResources(moduleToBuild, resourceType[ActionType.LoadResourceContent], modulesToBuild, fieldsToBuild, portalSettings);
            }

            var moduleResources = await _resourceMachine.RunAsync(_machineResources, context);

            //BundleMinifier.BundleAndMinify(
            //    new List<string>
            //    {
            //        "wwwroot/js/lib1.js",
            //        "wwwroot/js/lib2.js",
            //        "wwwroot/js/app.js"
            //    },
            //    "wwwroot/js/bundle.min.js"
            //);

            return CleanPageResources.Clean(
                 pageId.HasValue ? pageId.Value : 0,
                 dashboard.DashboardType == DashboardType.StandalonePanel,
                 moduleResources,
                 oldReesources
             );
        }

        private void ProcessGetResources(BuildModuleDto moduleToBuild, IEnumerable<BuildModuleResourceDto> resources)
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
                            ModuleId = moduleToBuild.Id,
                            ActionType = ActionType.GetResourcePath,
                            AddToResources = true,
                            ResourceFiles = items.Select(rf =>
                                 new MachineResourceFileInfo()
                                 {
                                     FilePath = rf.ResourcePath,
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
                BuildModuleDto moduleToBuild,
                IEnumerable<BuildModuleResourceDto> resourcesToBuild,
                IEnumerable<BuildModuleDto> modulesToBuild,
                IEnumerable<BuildModuleFieldDto> fieldsToBuild,
                PortalSettings portalSettings
            )
        {
            var lookup = resourcesToBuild.ToLookup(r => r.ResourceType == ResourceType.FieldTypeTemplate);
            var fieldTypeTemplates = lookup[true];
            if (fieldTypeTemplates != null && fieldTypeTemplates.Any())
            {
                _machineResources.Add(
                    new MachineResourceInfo()
                    {
                        ModuleId = moduleToBuild.Id,
                        ActionType = ActionType.LoadResourceContent,
                        ResourceFiles = fieldTypeTemplates.Select(
                            r => new MachineResourceFileInfo()
                            {
                                FilePath = r.ResourcePath,
                                EntryType = r.EntryType,
                                Additional = r.Additional,
                                CacheKey = r.CacheKey,
                                ContinueOnError = true
                            }).ToList(),
                        OperationType = OperationType.MergeResourceFiles,
                        MergeStrategy = new ResourceMergeStrategy()
                        {
                            MergedCallback = args =>
                            {
                                var fieldTypes = args[0] as IDictionary<(string entryType, string entryAdditional), string>;
                                return _buildModuleLayout.BuildLayout(moduleToBuild.LayoutTemplate, fieldTypes, fieldsToBuild);
                            },
                            MergedOutputFilePath = $@"{moduleToBuild.BuildPath}\module--{moduleToBuild.ModuleName}.html"
                                .Replace("[BUILDPATH]", portalSettings.HomeSystemDirectoryMapPath + @"business-engine\")
                        },
                    }
                );
            }

            var dict = lookup[false].GroupBy(r => r.ResourceType)
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
                            ModuleId = moduleToBuild.Id,
                            ActionType = ActionType.LoadResourceContent,
                            AddToResources = true,
                            ResourceFiles = items.Select(
                            r => new MachineResourceFileInfo()
                            {
                                FilePath = r.ResourcePath,
                                EntryType = r.EntryType,
                                Additional = r.Additional,
                                CacheKey = r.CacheKey,
                                ContinueOnError = true

                            }).ToList(),
                            Condition = firstItem.Condition,
                            OperationType = OperationType.MergeResourceFiles,
                            MergeStrategy = new ResourceMergeStrategy()
                            {
                                MergedOutputFilePath = $@"{moduleToBuild.BuildPath}\{GetModuleFilename(resourceType)}"
                                .Replace("[BUILDPATH]", portalSettings.HomeSystemDirectoryMapPath + @"business-engine\"),
                                MergedResourcePath = $@"{moduleToBuild.BuildPath}/{GetModuleFilename(resourceType)}"
                                .Replace("[BUILDPATH]", portalSettings.HomeSystemDirectory + @"business-engine/").Replace(@"\", "/")
                            },
                            ContinueOnError = true,
                            Order = _machineResources.Count + 1
                        }
                    );
                }
            }
        }

        private object GetModuleFilename(ResourceType resourceType)
        {
            switch (resourceType)
            {
                case ResourceType.ModuleActionScript:
                    return "modules--action.js";
                case ResourceType.FieldTypeScript:
                    return "fields--script.js";
                case ResourceType.FieldDirectiveScript:
                    return "fields--directive.js";
                case ResourceType.FieldTypeTheme:
                    return "fields--theme.css";
                default:
                    return string.Empty;
            }
        }
    }
}
