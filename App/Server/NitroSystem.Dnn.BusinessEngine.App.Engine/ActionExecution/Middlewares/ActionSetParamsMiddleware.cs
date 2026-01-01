using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<ActionResponse> InvokeAsync(IEngineContext context, ActionRequest request, Func<Task<ActionResponse>> next)
        {
            var finalizedParams = new List<ActionParamDto>();

            foreach (var item in request.Action.Params ?? Enumerable.Empty<ActionParamDto>())
            {
                var expr = item.ParamValue as string;

                if (string.IsNullOrEmpty(expr) && request.ExtraParams != null && request.ExtraParams.TryGetValue(item.ParamName, out var val))
                    item.ParamValue = val;
                else
                    item.ParamValue = !string.IsNullOrEmpty(expr)
                        ? _expressionService.Evaluate(expr, request.ModuleData)
                        : item.ParamValue;

                finalizedParams.Add(item);
                context.Set<List<ActionParamDto>>("ParsedParams", finalizedParams);
            }

            var result = await next();
            return result;
        }
    }
}
