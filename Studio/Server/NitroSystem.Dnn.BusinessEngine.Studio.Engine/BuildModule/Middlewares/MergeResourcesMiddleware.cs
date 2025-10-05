using System;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.BuildModuleEngine;
using NitroSystem.Dnn.BusinessEngine.Abstractions.BuildModuleEngine.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Abstractions.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Middlewares
{
    public class MergeResourcesMiddleware : IEngineMiddleware<BuildModuleRequest, BuildModuleResponse>
    {
        private readonly IMergeResourcesService _service;

        public MergeResourcesMiddleware(IMergeResourcesService service)
        {
            _service = service;
        }

        public async Task<EngineResult<BuildModuleResponse>> InvokeAsync(EngineContext context, BuildModuleRequest request, Func<Task<EngineResult<BuildModuleResponse>>> next)
        {
            var layoutResults = await _service.MergeResourcesAsync(request.Module.Resources);

            context.Set<string>("ModuleScripts", layoutResults.Scripts);
            context.Set<string>("ModuleStyles", layoutResults.Styles);

            var result = await next();
            return result;
        }
    }
}
