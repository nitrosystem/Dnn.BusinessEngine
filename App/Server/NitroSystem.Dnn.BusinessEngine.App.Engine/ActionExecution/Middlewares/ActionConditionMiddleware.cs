using System;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution.Middlewares
{
    public class ActionConditionMiddleware : IEngineMiddleware<ActionRequest, ActionResponse>
    {
        private readonly IExpressionService _service;

        public ActionConditionMiddleware(IExpressionService service)
        {
            _service = service;
        }

        public async Task<ActionResponse> InvokeAsync(IEngineContext context, ActionRequest request, Func<Task<ActionResponse>> next)
        {
            //if (_service.Evaluate<bool>(request.Action.Conditions, request.ModuleData))
            //    context.CancellationTokenSource.Cancel();
            //else
                context.Set<bool>("ConditionsIsTrue", true);

            var result = await next();
            return result;
        }
    }

}
