using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.Enums;

namespace NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecutionEngine.Middlewares
{
    public class ActionSetResultMiddleware : IEngineMiddleware<ActionRequest, ActionResponse>
    {
        private readonly IExpressionService _service;

        public ActionSetResultMiddleware(IExpressionService service)
        {
            _service = service;
        }

        public async Task<EngineResult<ActionResponse>> InvokeAsync(IEngineContext context, ActionRequest request, Func<Task<EngineResult<ActionResponse>>> next)
        {
            var ctx = context as ActionExecutionContext;
            var actionResult = ctx.Get<ActionResult>("ActionResult");

            WithServiceResult(ctx.ModuleData, actionResult.Data, () =>
            {
                foreach (var item in ctx.Action.Results)
                {
                    if (string.IsNullOrWhiteSpace(item.Conditions) || _service.Evaluate<bool>(item.Conditions, ctx.ModuleData))
                    {
                        var value = _service.Evaluate(item.RightExpression, ctx.ModuleData);
                        var setter = _service.BuildDataSetter(item.LeftExpression, ctx.ModuleData);

                        setter(value);
                    }
                }
            });

            ctx.Result = new ActionResult() { Status = ActionResultStatus.Successful };

            var result = await next();
            return result;
        }

        private void WithServiceResult(
            ConcurrentDictionary<string, object> moduleData,
            object resultData,
            Action action
        )
        {
            const string key = "_ServiceResult";
            moduleData[key] = resultData;

            try
            {
                action();
            }
            finally
            {
                moduleData.TryRemove(key, out _);
            }
        }
    }
}
