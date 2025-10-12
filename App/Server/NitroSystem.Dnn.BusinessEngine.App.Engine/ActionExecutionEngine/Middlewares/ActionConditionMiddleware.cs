using System;
using System.Threading;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution;

namespace NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecutionEngine.Middlewares
{
    public class ActionConditionMiddleware : IEngineMiddleware<ActionRequest, ActionResponse>
    {
        private readonly IExpressionService _service;

        public ActionConditionMiddleware(IExpressionService service)
        {
            _service = service;
        }

        public async Task<EngineResult<ActionResponse>> InvokeAsync(IEngineContext context, ActionRequest request, Func<Task<EngineResult<ActionResponse>>> next)
        {
            var ctx = context as ActionExecutionContext;

            if (_service.Evaluate<bool>(ctx.Action.Conditions, ctx.ModuleData))
            {
                ctx.CancellationTokenSource.Cancel(); // حالا Cancel در دسترسه
                return EngineResult<ActionResponse>.Failure("Condition failed.");
            }

            var result = await next();
            return result;
        }
    }

}
