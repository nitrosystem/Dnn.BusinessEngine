using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Dto;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Services
{
    public class ResourceAggregatorService : IResourceAggregatorService
    {
        public async Task<BuildModuleResponse> FinalizeResourcesAsync(BuildModuleRequest request, IEngineContext context)
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

            result.IsSuccess = true;
            result.FinalizedResources = request.Module.ExternalResources.Concat(systemResources);
            return result;
        }
    }
}
