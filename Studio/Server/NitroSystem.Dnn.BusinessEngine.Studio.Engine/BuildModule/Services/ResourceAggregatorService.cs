using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Dto;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Services
{
    public class ResourceAggregatorService : IResourceAggregatorService
    {
        public async Task<BuildModuleResponse> FinalizeResourcesAsync(BuildModuleRequest request, IEngineContext context,IEngineNotifier engineNotifier)
        {
            var ctx = context as EngineContext;
            var result = new BuildModuleResponse();
            var moduleLayoutTemplate = ctx.Get<string>("ModuleLayoutTemplate");
            var scripts = ctx.Get<string>("ModuleScripts");
            var styles = ctx.Get<string>("ModuleStyles");
            var outputDirectory = ctx.Get<string>("OutputDirectory");
            var outputRelativePath = ctx.Get<string>("OutputRelativePath");
            var moduleKebabName = StringHelper.ToKebabCase(request.ModuleName);

            var fileTasks = new[]
            {
                FileUtil.WriteFileContentAsync($"{outputDirectory}/{moduleKebabName}.html", moduleLayoutTemplate),
                FileUtil.WriteFileContentAsync($"{outputDirectory}/{moduleKebabName}.js", scripts),
                FileUtil.WriteFileContentAsync($"{outputDirectory}/{moduleKebabName}.css", styles)
            };

            if (!string.IsNullOrWhiteSpace(request.Module.PreloadingTemplate))
                fileTasks.Append(FileUtil.WriteFileContentAsync($"{outputDirectory}/{moduleKebabName}.preloader.html", request.Module.PreloadingTemplate));

            await Task.WhenAll(fileTasks);

            int count = request.Module.ExternalResources.Count();

            var systemResources = new List<ModuleResourceDto>();
            systemResources.Add(new ModuleResourceDto()
            {
                ModuleId = request.ModuleId.Value,
                ResourceContentType = ResourceContentType.Css,
                ResourcePath = $"{outputRelativePath}/{moduleKebabName}.css",
                LoadOrder = ++count
            });

            systemResources.Add(new ModuleResourceDto()
            {
                ModuleId = request.ModuleId.Value,
                ResourceContentType = ResourceContentType.Js,
                ResourcePath = $"{outputRelativePath}/{moduleKebabName}.js",
                LoadOrder = ++count
            });

            engineNotifier.PushingNotification(request.Module.ScenarioName,
                new
                {
                    Type = "ActionCenter",
                    TaskId = $"{request.ModuleId}-BuildModule",
                    Message = $"Finalized resources for {request.ModuleName} module",
                    Percent = 95
                }
            );

            result.IsSuccess = true;
            result.FinalizedResources = request.Module.ExternalResources.Concat(systemResources);
            return result;
        }
    }
}
