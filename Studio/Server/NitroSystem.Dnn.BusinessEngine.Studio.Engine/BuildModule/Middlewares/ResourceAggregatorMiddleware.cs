using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Dto;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Middlewares
{
    public class ResourceAggregatorMiddleware : IEngineMiddleware<BuildModuleRequest, BuildModuleResponse>
    {
        public async Task<BuildModuleResponse> InvokeAsync(IEngineContext context, BuildModuleRequest request, Func<Task<BuildModuleResponse>> next)
        {
            //var response = await _workflow.ExecuteTaskAsync<BuildModuleResponse>(request.ModuleId.Value.ToString(), request.UserId,
            //        "BuildModuleWorkflow", "BuildModule", "ResourceAggregatorMiddleware", false, true, false,
            //       (Expression<Func<Task<BuildModuleResponse>>>)(() => _service.FinalizeResourcesAsync(request, context))
            //    );

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

            if (!string.IsNullOrWhiteSpace(request.Module.PreloadingTemplate))
                fileTasks.Append(FileUtil.WriteFileContentAsync($"{outputDirectory}/{moduleKebabName}.preloader.html", request.Module.PreloadingTemplate));

            await Task.WhenAll(fileTasks);

            int count = request.Module.ExternalResources.Count();

            var moduleResources = new List<ModuleResourceDto>();
            moduleResources.Add(new ModuleResourceDto()
            {
                ModuleId = request.ModuleId.Value,
                ResourceContentType = ResourceContentType.Css,
                ResourcePath = $"{outputRelativePath}/{moduleKebabName}.css",
                LoadOrder = ++count
            });

            moduleResources.Add(new ModuleResourceDto()
            {
                ModuleId = request.ModuleId.Value,
                ResourceContentType = ResourceContentType.Js,
                ResourcePath = $"{outputRelativePath}/{moduleKebabName}.js",
                LoadOrder = ++count
            });

            //engineNotifier.PushingNotification(request.Module.ScenarioName,
            //    new
            //    {
            //        Type = "ActionCenter",
            //        TaskId = $"{request.ModuleId}-BuildModule",
            //        Message = $"Finalized resources for {request.ModuleName} module",
            //        Percent = 95
            //    }
            //);

            result.IsSuccess = true;
            result.FinalizedResources = request.Module.ExternalResources.Concat(moduleResources);
            return result;
        }
    }
}
