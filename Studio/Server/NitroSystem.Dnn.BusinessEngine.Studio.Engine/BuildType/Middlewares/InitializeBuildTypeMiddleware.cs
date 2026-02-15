using System;
using System.IO;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Globals;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.TypeBuilder.Middlewares
{
    public class InitializeBuildTypeMiddleware : IEngineMiddleware<BuildTypeRequest, BuildTypeResponse>
    {
        public async Task<BuildTypeResponse> InvokeAsync(IEngineContext context, BuildTypeRequest request, Func<Task<BuildTypeResponse>> next, Action<string, double> progress = null)
        {
            var scenarioFolder = StringHelper.ToKebabCase(request.ScenarioName);
            var relativePath = $"{request.BasePath}business-engine/{scenarioFolder}/app-model-types";
            var outputDirectory = Constants.MapPath($@"{request.BasePath}business-engine\{scenarioFolder}\app-model-types");

            if (!Directory.Exists(outputDirectory)) Directory.CreateDirectory(outputDirectory);

            context.Set("OutputDirectory", outputDirectory);
            context.Set("OutputRelativePath", relativePath);

            var result = await next();
            return result;
        }
    }
}
