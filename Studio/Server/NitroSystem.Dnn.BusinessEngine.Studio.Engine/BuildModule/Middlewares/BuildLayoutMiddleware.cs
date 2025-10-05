using System;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.BuildModuleEngine;
using NitroSystem.Dnn.BusinessEngine.Abstractions.BuildModuleEngine.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Abstractions.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Middlewares
{
    public class BuildLayoutMiddleware : IEngineMiddleware<BuildModuleRequest, BuildModuleResponse>
    {
        private readonly IBuildLayoutService _service;

        public BuildLayoutMiddleware(IBuildLayoutService service)
        {
            _service = service;
        }

        public async Task<EngineResult<BuildModuleResponse>> InvokeAsync(EngineContext context, BuildModuleRequest request, Func<Task<EngineResult<BuildModuleResponse>>> next)
        {
            var layout = await _service.BuildLayoutAsync(request.Module.LayoutTemplate, request.Module.Fields);

            context.Set<string>("ModuleLayoutTemplate", layout);

            var result = await next();
            return result;
        }
    }
}
