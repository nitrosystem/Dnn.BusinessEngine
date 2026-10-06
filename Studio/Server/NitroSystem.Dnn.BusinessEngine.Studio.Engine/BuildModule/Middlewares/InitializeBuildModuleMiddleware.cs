using System;
using System.IO;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Middlewares
{
    public class InitializeBuildModuleMiddleware : IEngineMiddleware<BuildModuleRequest, BuildModuleResponse>
    {
        private readonly IModuleService _moduleService;

        public InitializeBuildModuleMiddleware(IModuleService moduleService)
        {
            _moduleService = moduleService;
        }

        public async Task<BuildModuleResponse> InvokeAsync(IEngineContext context, BuildModuleRequest request, Func<Task<BuildModuleResponse>> next, Action<string, double> progress = null)
        {
            var module = request.Module;
            var scenarioFolder = StringHelper.ToKebabCase(module.ScenarioName);
            var parentFolder = !string.IsNullOrEmpty(module.ParentModuleName)
                ? StringHelper.ToKebabCase(module.ParentModuleName) + @"\"
                : string.Empty;
            var moduleFolder = StringHelper.ToKebabCase(module.ModuleName);
            var outputDirectory= $@"{request.BasePath}{scenarioFolder}\{parentFolder}{moduleFolder}";
            var relativeDirectory = GlobalHelper.RelativePath(outputDirectory);
            if (!Directory.Exists(outputDirectory)) Directory.CreateDirectory(outputDirectory);

            context.Set("OutputDirectory", outputDirectory);
            context.Set("OutputRelativePath", relativeDirectory);

            progress.Invoke($"Initialized for build {request.Module.ModuleName} module", 5);

            var result = await next();
            return result;
        }
    }
}
