using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.App.Engine.ActionEngine;

namespace NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecutionEngine.Services
{
    public class ActionWorker : IActionWorker
    {
        private readonly IActionService _actionService;
        private readonly IExpressionService _expressionService;
        private readonly IServiceLocator _serviceLocator;

        public ActionWorker(
            IActionService actionService,
            IExpressionService expressionService,
            IServiceLocator serviceLocator)
        {
            _actionService = actionService;
            _expressionService = expressionService;
            _serviceLocator = serviceLocator;
        }

        public async Task<ActionResult> CallAction(IEngineContext context)
        {
            var ctx = context as ActionExecutionContext;

            var result = new ActionResult();

            ctx.Action.Params = ParseParams(ctx.Action.Params, ctx);

            IActionExecutor actionController = await GetActionExtensionInstance(ctx.Action.ActionType);

            try
            {
                result = await actionController.ExecuteAsync(ctx);
            }
            catch (Exception)
            {
                result.Status = ActionResultStatus.Error;
            }

            return result;
        }

        private List<ActionParamDto> ParseParams(IEnumerable<ActionParamDto> actionParams, ActionExecutionContext context)
        {
            var finalizedParams = new List<ActionParamDto>();

            foreach (var item in actionParams)
            {
                var expr = item.ParamValue as string;
                var value = !string.IsNullOrEmpty(expr)
                    ? _expressionService.Evaluate(expr, context.ModuleData)
                    : item.ParamValue;

                item.ParamValue = value;
                finalizedParams.Add(item);
            }

            return finalizedParams;
        }

        private async Task<IActionExecutor> GetActionExtensionInstance(string actionType)
        {
            var businessControllerClass = await _actionService.GetBusinessControllerClass(actionType);

            return _serviceLocator.GetInstance<IActionExecutor>(businessControllerClass);
        }
    }
}
