using System;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Middlewares
{
    public class MergeResourcesMiddleware : IEngineMiddleware<BuildModuleRequest, BuildModuleResponse>
    {
        private readonly IMergeResourcesService _service;

        public MergeResourcesMiddleware(IMergeResourcesService service)
        {
            _service = service;
        }

        public async Task<EngineResult<BuildModuleResponse>> InvokeAsync(IEngineContext context, BuildModuleRequest request, Func<Task<EngineResult<BuildModuleResponse>>> next, IEngineNotifier engineNotifier)
        {
            var ctx = context as EngineContext;

            var layoutResults = await _service.MergeResourcesAsync(request.Module, request.UserId, request.Module.Resources, engineNotifier);

            ctx.Set<string>("ModuleScripts", layoutResults.Scripts);
            ctx.Set<string>("ModuleStyles", layoutResults.Styles);

            var result = await next();
            return result;
        }
    }
}
