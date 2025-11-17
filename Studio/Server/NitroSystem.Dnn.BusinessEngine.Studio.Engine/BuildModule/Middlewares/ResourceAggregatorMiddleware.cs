using System;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Core.Workflow;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Middlewares
{
    public class ResourceAggregatorMiddleware : IEngineMiddleware<BuildModuleRequest, BuildModuleResponse>
    {
        private readonly IResourceAggregatorService _service;
        private readonly WorkflowEventManager _eventManager;

        public ResourceAggregatorMiddleware(IResourceAggregatorService service, WorkflowEventManager eventManager)
        {
            _service = service;
            _eventManager = eventManager;
        }

        public async Task<EngineResult<BuildModuleResponse>> InvokeAsync(IEngineContext context, BuildModuleRequest request, Func<Task<EngineResult<BuildModuleResponse>>> next)
        {
            var response = await _eventManager.ExecuteTaskAsync<BuildModuleResponse>(request.ModuleId.Value.ToString(), request.UserId,
                    "BuildModuleWorkflow", "BuildModule", "ResourceAggregatorMiddleware", false, true, false,
                   (Expression<Func<Task<BuildModuleResponse>>>)(() => _service.FinalizeResourcesAsync(request, context))
                );

            return EngineResult<BuildModuleResponse>.Success(response);
        }
    }
}
