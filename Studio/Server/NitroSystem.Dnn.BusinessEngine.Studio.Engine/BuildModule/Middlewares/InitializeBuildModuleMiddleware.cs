using System;
using System.IO;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Globals;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Middlewares
{
    public class InitializeBuildModuleMiddleware : IEngineMiddleware<BuildModuleRequest, BuildModuleResponse>
    {
        public async Task<BuildModuleResponse> InvokeAsync(IEngineContext context, BuildModuleRequest request, Func<Task<BuildModuleResponse>> next)
        {
            if (request.Module.Wrapper == ModuleWrapper.Dashboard)
                request.BasePath = Path.Combine(request.BasePath, StringHelper.ToKebabCase(request.Module.ParentModuleName));

            var moduleFolder = StringHelper.ToKebabCase(request.Module.ModuleName);
            var outputDirectory = Constants.MapPath($"{request.BasePath}/{moduleFolder}");
            var relativeDirectory = $"{request.BasePath}/{moduleFolder}";

            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            context.Set("OutputDirectory", outputDirectory);
            context.Set("OutputRelativePath", relativeDirectory);

            //PushingNotification(request.Module.ScenarioName,
            //    new
            //    {
            //        Type = "ActionCenter",
            //        TaskId = $"{request.ModuleId}-BuildModule",
            //        Message = $"Initialized for build {request.ModuleName} module",
            //        Percent = 5
            //    }
            //);

            var result = await next();
            return result;
        }
    }
}
