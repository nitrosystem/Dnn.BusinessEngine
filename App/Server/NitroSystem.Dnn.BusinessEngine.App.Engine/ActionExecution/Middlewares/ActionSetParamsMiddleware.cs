using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;

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
                    ConcurrentDictionary<string, object> moduleData = request.ModuleData;

                    if (context.TryGet<ConcurrentDictionary<string, object>>("ModuleData", out var data) && data != null)
                        moduleData = data;

                    item.ParamValue = !string.IsNullOrEmpty(expr)
                        ? _expressionService.Evaluate(expr, moduleData)
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
