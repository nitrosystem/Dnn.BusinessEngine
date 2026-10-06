using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ExpressionEngine;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Models;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Expressions;

namespace NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution.Middlewares
{
    public class ActionConditionMiddleware : IEngineMiddleware<ActionRequest, ActionResponse>
    {
        public async Task<ActionResponse> InvokeAsync(IEngineContext context, ActionRequest request, Func<Task<ActionResponse>> next, Action<string, double> progress = null)
        {
            var action = request.Action;

            if (!string.IsNullOrWhiteSpace(action.ActionConditionsDsl))
            {
                var moduleData = request.ModuleData;
                moduleData.AddOrUpdate("_ConditionIsTrue", false, (key, value) => value);

                var tokenizer = new Tokenizer(action.ActionConditionsDsl);
                List<Token> tokens = tokenizer.Tokenize();
                var parser = new DslParser(tokens);
                DslScript script = parser.ParseScript();
                var dslContext = new ExpressionContext(moduleData);
                var compiler = new ExpressionCompiler();
                var executor = new DslExecutor(compiler);
                executor.Execute(script, dslContext);

                if (!moduleData.TryGetValue("_ConditionIsTrue", out var isTrue) || !(bool)isTrue)
                {
                    moduleData.TryRemove("_ConditionIsTrue", out _);
                    return new ActionResponse() { ConditionIsNotTrue = true, ModuleData = moduleData };
                }

                moduleData.TryRemove("_ConditionIsTrue", out _);
            }

            var result = await next();
            return result;
        }
    }
}
