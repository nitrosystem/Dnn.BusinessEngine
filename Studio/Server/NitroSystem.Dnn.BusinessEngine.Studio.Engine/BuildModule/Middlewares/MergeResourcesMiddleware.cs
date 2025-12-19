using System;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Middlewares
{
    public class MergeResourcesMiddleware : IEngineMiddleware<BuildModuleRequest, BuildModuleResponse>
    {
        private readonly IMergeResourcesService _service;

        public MergeResourcesMiddleware(IMergeResourcesService service)
        {
            _service = service;
        }

        public async Task<BuildModuleResponse> InvokeAsync(IEngineContext context, BuildModuleRequest request, Func<Task<BuildModuleResponse>> next)
        {
            var layoutResults = await _service.MergeResourcesAsync(request.Module, request.UserId, request.Module.Resources);

            context.Set<string>("ModuleScripts", layoutResults.Scripts);
            context.Set<string>("ModuleStyles", layoutResults.Styles);

            var result = await next();
            return result;
        }
    }
}
