using System;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Middlewares
{
    public class DeleteOldResourcesMiddleware : IEngineMiddleware<BuildModuleRequest, BuildModuleResponse>
    {
        private readonly IModuleService _moduleService;

        public DeleteOldResourcesMiddleware(IModuleService moduleService)
        {
            _moduleService = moduleService;
        }

        public async Task<BuildModuleResponse> InvokeAsync(IEngineContext context, BuildModuleRequest request, Func<Task<BuildModuleResponse>> next, Action<string, double> progress = null)
        {
            await _moduleService.DeleteModuleResourcesAsync(request.Module.Id);

            progress.Invoke($"Deleted old resources of {request.Module.ModuleName} module", 10);

            var result = await next();
            return result;
        }
    }
}
