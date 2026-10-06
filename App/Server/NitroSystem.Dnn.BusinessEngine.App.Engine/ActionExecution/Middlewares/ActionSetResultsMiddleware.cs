using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ExpressionEngine;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Models;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Expressions;
using NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution.Enums;

namespace NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution.Middlewares
{
    public class ActionSetResultsMiddleware : IEngineMiddleware<ActionRequest, ActionResponse>
    {
        public async Task<ActionResponse> InvokeAsync(IEngineContext context, ActionRequest request, Func<Task<ActionResponse>> next, Action<string, double> progress = null)
        {
            await Task.Yield();

            var action = request.Action;

            if (!context.TryGet<ConcurrentDictionary<string, object>>("ModuleData", out var moduleData))
                moduleData = request.ModuleData;

            var isRequiredToUpdateData = false;
            var result = new ActionResponse();

            if (context.TryGet<object>("ResultData", out var actionResult))
            {
                if (request.Action.IsRedirectable)
                    result.RedirectUrl = actionResult?.ToString();
                else if (!string.IsNullOrWhiteSpace(action.ActionResultsDsl))
                {
                    WithServiceResult(moduleData, actionResult, () =>
                    {
                        var tokenizer = new Tokenizer(action.ActionResultsDsl);
                        List<Token> tokens = tokenizer.Tokenize();

                        var parser = new DslParser(tokens);
                        DslScript script = parser.ParseScript();

                        var dslContext = new ExpressionContext(moduleData);
                        var compiler = new ExpressionCompiler();
                        var executor = new DslExecutor(compiler);
                        executor.Execute(script, dslContext);
                        isRequiredToUpdateData = true;
                    });

                    result.ModuleData = moduleData;
                }
            }

            result.IsRequiredToUpdateData = isRequiredToUpdateData;
            result.Status = ActionResultStatus.Successful;
            return result;
        }

        private void WithServiceResult(ConcurrentDictionary<string, object> moduleData, object resultData, Action action)
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
