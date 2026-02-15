using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Expressions;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Models;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine;

namespace NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution.Middlewares
{
    public class ActionSetResultsMiddleware : IEngineMiddleware<ActionRequest, ActionResponse>
    {
        private readonly IExpressionService _service;

        public ActionSetResultsMiddleware(IExpressionService service)
        {
            _service = service;
        }

        public async Task<ActionResponse> InvokeAsync(IEngineContext context, ActionRequest request, Func<Task<ActionResponse>> next, Action<string, double> progress = null)
        {
            await Task.Yield();

            var action = request.Action;
            var moduleData = request.ModuleData;
            var isRequiredToUpdateData = false;

            if (context.TryGet<object>("ResultData", out var actionResult) && !string.IsNullOrWhiteSpace(action.ActionResultsDsl))
            {
                WithServiceResult(moduleData, actionResult, () =>
                {
                    var tokenizer = new Tokenizer(action.ActionResultsDsl);
                    List<Token> tokens = tokenizer.Tokenize();

                    var parser = new DslParser(tokens);
                    DslScript script = parser.ParseScript();

                    var dslContext = new DslContext(moduleData);

                    var compiler = new ExpressionCompiler(dslContext);
                    var executor = new DslExecutor(compiler);

                    executor.Execute(script, dslContext);

                    isRequiredToUpdateData = true;
                });
            }

            var result = new ActionResponse()
            {
                IsRequiredToUpdateData = isRequiredToUpdateData,
                Status = ActionResultStatus.Successful,
                ModuleData = moduleData
            };
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
