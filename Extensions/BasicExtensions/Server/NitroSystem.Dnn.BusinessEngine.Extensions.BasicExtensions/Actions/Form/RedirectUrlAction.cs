using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Dto;
using NitroSystem.Dnn.BusinessEngine.Core.ExpressionEngine;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Expressions;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Models;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Actions.Form
{
    public class RedirectUrlAction : IActionExecutor
    {
        public async Task<object> ExecuteAsync(ActionDto action, ConcurrentDictionary<string, object> moduleData)
        {
            await Task.Yield();

            if (action.Settings.TryGetValue("DslScript", out var dslScript) && dslScript != null)
            {
                var tokenizer = new Tokenizer(dslScript.ToString());
                List<Token> tokens = tokenizer.Tokenize();

                var parser = new DslParser(tokens);
                DslScript script = parser.ParseScript();

                moduleData.AddOrUpdate("_Url", string.Empty, (key, value) => value);
                var dslContext = new ExpressionContext(moduleData);

                var compiler = new ExpressionCompiler();
                var executor = new DslExecutor(compiler);
                executor.Execute(script, dslContext);

                if (!moduleData.TryGetValue("_Url", out var url) || !string.IsNullOrEmpty((string)url))
                {
                    moduleData.TryRemove("_Url", out _);
                    return url;
                }
            }

            return "";
        }
    }
}
