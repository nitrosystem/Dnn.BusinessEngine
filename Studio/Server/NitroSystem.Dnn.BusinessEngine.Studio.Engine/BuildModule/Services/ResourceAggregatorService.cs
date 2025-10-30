using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ModuleBuilder.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Shared.Extensions;
using NitroSystem.Dnn.BusinessEngine.Shared.Globals;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Services
{
    public class ResourceAggregatorService : IResourceAggregatorService
    {
        private readonly IModuleService _moduleService;

        public ResourceAggregatorService(IModuleService moduleService)
        {
            _moduleService = moduleService;
        }

        public async Task<BuildModuleResponse> FinalizeResourcesAsync(BuildModuleRequest request, EngineContext context)
        {
            var result = new BuildModuleResponse();
            var moduleLayoutTemplate = context.Get<string>("ModuleLayoutTemplate");
            var scripts = context.Get<string>("ModuleScripts");
            var styles = context.Get<string>("ModuleStyles");
            var outputDirectory = context.Get<string>("OutputDirectory");
            var outputRelativePath = context.Get<string>("OutputRelativePath");
            var moduleKebabName = StringHelper.ToKebabCase(request.ModuleName);

            var fileTasks = new[]
            {
                FileUtil.WriteFileContentAsync($"{outputDirectory}/{moduleKebabName}.html", moduleLayoutTemplate),
                FileUtil.WriteFileContentAsync($"{outputDirectory}/{moduleKebabName}.js", scripts),
                FileUtil.WriteFileContentAsync($"{outputDirectory}/{moduleKebabName}.css", styles)
            };

            await Task.WhenAll(fileTasks);

            int count = request.Module.ExternalResources.Count();

            var systemResources = new List<ModuleResourceDto>();
            systemResources.Add(new ModuleResourceDto()
            {
                ModuleId = request.ModuleId.Value,
                ResourceContentType = ModuleResourceContentType.Css,
                ResourcePath = $"{outputRelativePath}/{moduleKebabName}.css",
                LoadOrder = ++count
            });

            systemResources.Add(new ModuleResourceDto()
            {
                ModuleId = request.ModuleId.Value,
                ResourceContentType = ModuleResourceContentType.Js,
                ResourcePath = $"{outputRelativePath}/{moduleKebabName}.js",
                LoadOrder = ++count
            });

            var resourcesFinalized = request.Module.ExternalResources.Concat(systemResources);

            await _moduleService.BulkInsertModuleOutputResourcesAsync(request.Module.SitePageId, resourcesFinalized);

            result.Success = true;
            return result;
        }
    }
}
