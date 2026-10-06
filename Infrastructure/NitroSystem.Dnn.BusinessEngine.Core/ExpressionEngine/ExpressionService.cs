using System;
using NitroSystem.Dnn.BusinessEngine.Core.ExpressionEngine.Base;
using NitroSystem.Dnn.BusinessEngine.Core.ExpressionEngine.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Core.ExpressionEngine
{
    public sealed class ExpressionService : ExpressionCompilerBase, IExpressionService
    {
        public object Evaluate(string expression, IExpressionContext context)
        {
            var expr = new Exp();
            expr.ExpressionPath = expression;
            var compiled = Compile(expr, context);
            return compiled(context);
        }

        public T Evaluate<T>(string expression, IExpressionContext context)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return default;

            var result = Evaluate(expression, context);
            if (result is T typed)
                return typed;

            // attemp for dynamic conversion if types differ
            try
            {
                return (T)Convert.ChangeType(result, typeof(T));
            }
            catch
            {
                return default;
            }
        }
    }
}
