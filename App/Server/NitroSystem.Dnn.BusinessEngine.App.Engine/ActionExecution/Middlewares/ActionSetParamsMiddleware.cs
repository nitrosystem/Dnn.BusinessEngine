using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Dto;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ExpressionEngine;
using NitroSystem.Dnn.BusinessEngine.Core.ExpressionEngine.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution.Middlewares
{
    public class ActionSetParamsMiddleware : IEngineMiddleware<ActionRequest, ActionResponse>
    {
        private readonly IExpressionService _expressionService;
        public ActionSetParamsMiddleware(IExpressionService expressionService)
        {
            _expressionService = expressionService;
        }

        public async Task<ActionResponse> InvokeAsync(IEngineContext context, ActionRequest request, Func<Task<ActionResponse>> next, Action<string, double> progress = null)
        {
            var finalizedParams = new List<ActionParamDto>();

            foreach (var item in request.Action.Params ?? Enumerable.Empty<ActionParamDto>())
            {
                var expr = item.ParamValue as string;
                if (string.IsNullOrEmpty(expr) && request.ExtraParams != null && request.ExtraParams.TryGetValue(item.ParamName, out var val))
                    item.ParamValue = val;
                else
                {
                    var moduleData = request.ModuleData;
                    var expressionContext = new ExpressionContext(moduleData);
                    if (context.TryGet<ConcurrentDictionary<string, object>>("ModuleData", out var data) && data != null)
                        moduleData = data;

                    item.ParamValue = !string.IsNullOrEmpty(expr)
                        ? _expressionService.Evaluate(expr, expressionContext)
                        : item.ParamValue;
                }

                finalizedParams.Add(item);
                context.Set<List<ActionParamDto>>("ParsedParams", finalizedParams);
            }

            var result = await next();
            return result;
        }
    }
}
