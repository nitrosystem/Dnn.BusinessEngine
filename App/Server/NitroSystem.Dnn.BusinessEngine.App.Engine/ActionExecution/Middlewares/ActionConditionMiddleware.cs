using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Expressions;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Models;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using System.Collections.Concurrent;

namespace NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution.Middlewares
{
    public class ActionConditionMiddleware : IEngineMiddleware<ActionRequest, ActionResponse>
    {
        public async Task<ActionResponse> InvokeAsync(IEngineContext context, ActionRequest request, Func<Task<ActionResponse>> next, Action<string, string, double> progress = null)
        {
            var action = request.Action;

            if (!string.IsNullOrWhiteSpace(action.ActionConditionsDsl))
            {
                var tokenizer = new Tokenizer(action.ActionConditionsDsl);
                List<Token> tokens = tokenizer.Tokenize();

                var parser = new DslParser(tokens);
                DslScript script = parser.ParseScript();

                var moduleData = request.ModuleData;
                moduleData.AddOrUpdate("ConditionIsTrue", false, (key, value) => value);
                var dslContext = new DslContext(moduleData);

                var compiler = new ExpressionCompiler(dslContext);
                var executor = new DslExecutor(compiler);

                executor.Execute(script, dslContext);

                if (!moduleData.TryGetValue("ConditionIsTrue", out var isTrue) || !(bool)isTrue)
                {
                    moduleData.TryRemove("ConditionIsTrue", out _);

                    return new ActionResponse() { ConditionIsNotTrue = true };
                }

                moduleData.TryRemove("ConditionIsTrue", out _);
            }

            var result = await next();
            return result;
        }

        private class ConditionResult
        {
            public bool isTrue { get; set; }
        }
    }
}
