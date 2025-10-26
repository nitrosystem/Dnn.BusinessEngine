using System;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.Models;

namespace NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecutionEngine.Middlewares
{
    public class ActionWorkerMiddleware : IEngineMiddleware<ActionRequest, ActionResponse>
    {
        private readonly IActionWorker _actionWorker;

        public ActionWorkerMiddleware(IActionWorker actionWorker)
        {
            _actionWorker = actionWorker;
        }

        public async Task<EngineResult<ActionResponse>> InvokeAsync(IEngineContext context, ActionRequest request, Func<Task<EngineResult<ActionResponse>>> next)
        {
            var ctx = context as ActionExecutionContext;

            var actionResult = await _actionWorker.CallAction(ctx);
            ctx.Set<ActionResult>("ActionResult", actionResult);

            var result = await next();
            return result;
        }
    }
}
