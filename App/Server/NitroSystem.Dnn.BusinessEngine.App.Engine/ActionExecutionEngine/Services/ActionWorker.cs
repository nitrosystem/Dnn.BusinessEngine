using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataServices.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.DContractsto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.Enums;

namespace NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecutionEngine.Services
{
    public class ActionWorker : IActionWorker
    {
        private readonly IActionService _actionService;
        private readonly IExpressionService _expressionService;
        private readonly IActionCondition _actionCondition;
        private readonly IServiceLocator _serviceLocator;

        public ActionWorker(
            IActionService actionService,
            IExpressionService expressionService,
            IActionCondition actionCondition,
            IServiceLocator serviceLocator)
        {
            _actionService = actionService;
            _expressionService = expressionService;
            _actionCondition = actionCondition;
            _serviceLocator = serviceLocator;
        }

        public async Task<ActionResult> CallAction(ActionExecutionContext context)
        {
            var result = new ActionResult();

            context.Action.Params = ParseParams(context.Action.Params);

            IActionExecutor actionController = await GetActionExtensionInstance(context.Action.ActionType);

            try
            {
                result = await actionController.ExecuteAsync(context);
            }
            catch (Exception)
            {
                result.Status = ActionResultStatus.Error;
            }

            return result;
        }

        private List<ActionParamDto> ParseParams(IEnumerable<ActionParamDto> actionParams)
        {
            var finalizedParams = new List<ActionParamDto>();

            foreach (var item in actionParams)
            {
                var expr = item.ParamValue as string;
                var value = !string.IsNullOrEmpty(expr)
                    ? _expressionService.Evaluate(expr, null)
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
