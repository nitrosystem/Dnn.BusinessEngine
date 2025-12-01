using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Workflow;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Middlewares
{
    public class ResourceAggregatorMiddleware : IEngineMiddleware<BuildModuleRequest, BuildModuleResponse>
    {
        private readonly IResourceAggregatorService _service;
        private readonly WorkflowManager _workflow;

        public ResourceAggregatorMiddleware(IResourceAggregatorService service, WorkflowManager workflow)
        {
            _service = service;
            _workflow = workflow;
        }

        public async Task<EngineResult<BuildModuleResponse>> InvokeAsync(IEngineContext context, BuildModuleRequest request, Func<Task<EngineResult<BuildModuleResponse>>> next, IEngineNotifier engineNotifier)
        {
            var response = await _workflow.ExecuteTaskAsync<BuildModuleResponse>(request.ModuleId.Value.ToString(), request.UserId,
                    "BuildModuleWorkflow", "BuildModule", "ResourceAggregatorMiddleware", false, true, false,
                   (Expression<Func<Task<BuildModuleResponse>>>)(() => _service.FinalizeResourcesAsync(request, context, engineNotifier))
                );

            return EngineResult<BuildModuleResponse>.Success(response);
        }
    }
}
