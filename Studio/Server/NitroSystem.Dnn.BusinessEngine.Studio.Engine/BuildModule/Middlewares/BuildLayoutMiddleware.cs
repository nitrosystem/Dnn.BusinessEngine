using System;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Middlewares
{
    public class BuildLayoutMiddleware : IEngineMiddleware<BuildModuleRequest, BuildModuleResponse>
    {
        private readonly IBuildLayoutService _service;

        public BuildLayoutMiddleware(IBuildLayoutService service)
        {
            _service = service;
        }

        public async Task<BuildModuleResponse> InvokeAsync(IEngineContext context, BuildModuleRequest request, Func<Task<BuildModuleResponse>> next)
        {
            var layout = await _service.BuildLayoutAsync(request.Module, request.UserId);
            context.Set<string>("ModuleLayoutTemplate", layout);

            var result = await next();
            return result;
        }
    }
}
