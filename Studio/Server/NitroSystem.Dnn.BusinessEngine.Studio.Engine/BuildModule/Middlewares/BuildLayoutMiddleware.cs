using System;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Middlewares
{
    public class BuildLayoutMiddleware : IEngineMiddleware<BuildModuleRequest, BuildModuleResponse>
    {
        private readonly IBuildLayoutService _service;

        public BuildLayoutMiddleware(IBuildLayoutService service)
        {
            _service = service;
        }

        public async Task<EngineResult<BuildModuleResponse>> InvokeAsync(IEngineContext context, BuildModuleRequest request, Func<Task<EngineResult<BuildModuleResponse>>> next, IEngineNotifier engineNotifier)
        {
            var ctx = context as EngineContext;
            var workflow = (context as EngineContext)?.EventManager;

            var layout = await _service.BuildLayoutAsync(request.Module, request.UserId, engineNotifier);
            ctx.Set<string>("ModuleLayoutTemplate", layout);

            var result = await next();
            return result;
        }
    }
}
