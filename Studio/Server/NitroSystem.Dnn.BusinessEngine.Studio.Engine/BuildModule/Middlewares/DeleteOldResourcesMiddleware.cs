using System;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Middlewares
{
    public class DeleteOldResourcesMiddleware : IEngineMiddleware<BuildModuleRequest, BuildModuleResponse>
    {
        private readonly IModuleService _moduleService;

        public DeleteOldResourcesMiddleware(IModuleService moduleService)
        {
            _moduleService = moduleService;
        }

        public async Task<BuildModuleResponse> InvokeAsync(IEngineContext context, BuildModuleRequest request, Func<Task<BuildModuleResponse>> next)
        {
            //await _workflow.ExecuteTaskAsync<object>(request.ModuleId.Value.ToString(), request.UserId,
            //    "BuildModuleWorkflow", "BuildModule", "InitialEngine", false, true, true,
            //        (Expression<Func<Task>>)(() => _moduleService.DeleteModuleResourcesAsync(request.ModuleId.Value))
            //    );
            
            await _moduleService.DeleteModuleResourcesAsync(request.Module.Id);

            // PushingNotification(request.Module.ScenarioName,
            //    new
            //    {
            //        Type = "ActionCenter",
            //        TaskId = $"{request.ModuleId}-BuildModule",
            //        Message = $"Deleting old resources of {request.ModuleName} module",
            //        Percent = 15
            //    }
            //);

            var result = await next();
            return result;
        }
    }
}
