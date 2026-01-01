using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution.Middlewares
{
    public class ActionWorkerMiddleware : IEngineMiddleware<ActionRequest, ActionResponse>
    {
        private readonly IActionService _actionService;
        private readonly IServiceLocator _serviceLocator;
        public ActionWorkerMiddleware(IActionService actionService, IServiceLocator serviceLocator)
        {
            _actionService = actionService;
            _serviceLocator = serviceLocator;
        }

        public async Task<ActionResponse> InvokeAsync(IEngineContext context, ActionRequest request, Func<Task<ActionResponse>> next)
        {
            context.TryGet<List<ActionParamDto>>("ParsedParams", out var finalizedParams);

            request.Action.Params = finalizedParams;

            var actionController = await GetActionExtensionInstance(request.Action.ActionType);
            var actionResultData = await actionController.ExecuteAsync(request.Action);
            context.Set<object>("ResultData", actionResultData);

            var result = await next();
            return result;
        }

        private async Task<IActionExecutor> GetActionExtensionInstance(string actionType)
        {
            var businessControllerClass = await _actionService.GetBusinessControllerClass(actionType);
            return _serviceLocator.GetInstance<IActionExecutor>(businessControllerClass);
        }
    }
}
