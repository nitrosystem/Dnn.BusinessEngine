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
    public class BeforeExecuteActionMiddleware : IEngineMiddleware<ActionRequest, ActionResponse>
    {

        public async Task<ActionResponse> InvokeAsync(IEngineContext context, ActionRequest request, Func<Task<ActionResponse>> next, Action<string, double> progress = null)
        {
            await Task.Yield();

            var action = request.Action;
            var moduleData = request.ModuleData;

            if (!string.IsNullOrWhiteSpace(action.BeforeExecuteActionDsl))
            {
                var tokenizer = new Tokenizer(action.BeforeExecuteActionDsl);
                List<Token> tokens = tokenizer.Tokenize();

                var parser = new DslParser(tokens);
                DslScript script = parser.ParseScript();

                var dslContext = new ExpressionContext(moduleData);
                var compiler = new ExpressionCompiler();
                var executor = new DslExecutor(compiler);
                executor.Execute(script, dslContext);
                context.Set("ModuleData", moduleData);
            }

            var result = await next();
            return result;
        }
    }
}
