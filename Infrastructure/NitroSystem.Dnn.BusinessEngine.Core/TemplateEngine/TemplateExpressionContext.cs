using System;
using System.Collections.Concurrent;
using NitroSystem.Dnn.BusinessEngine.Core.ExpressionEngine.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ExpressionEngine.Functions;

namespace NitroSystem.Dnn.BusinessEngine.Core.TemplateEngine
{
    /// <summary>
    /// Bridges TemplateContext's dynamic scope stack (push/pop per loop iteration)
    /// to IExpressionContext, so #For and #If can be evaluated through the shared
    /// Core.ExpressionEngine pipeline instead of a template-only evaluator.
    ///
    /// Unlike ExpressionContext (DSL), roots here are not a fixed, pre-registered
    /// set — TemplateContext.TryGet walks a Stack<Dictionary<string,object>> that
    /// grows/shrinks per loop iteration, which is what makes nested #For loops and
    /// variable shadowing (inner "item" hiding outer "item") work correctly.
    /// </summary>
    public sealed class TemplateExpressionContext : IExpressionContext
    {
        private readonly TemplateContext _templateContext;
        private static readonly ConcurrentDictionary<string, Delegate> Functions = ExpressionFunctions.BuiltIn;

        public TemplateExpressionContext(TemplateContext templateContext)
        {
            _templateContext = templateContext;
        }

        public object GetRoot(string name)
        {
            _templateContext.TryGet(name, out var value);
            return value;
        }

        public Type GetRootType(string name)
        {
            return _templateContext.TryGet(name, out var value) && value != null
                ? value.GetType()
                : typeof(object);
        }

        public void SetRoot(string name, object value) =>
            throw new NotSupportedException(
                "Template scopes are managed via TemplateContext.PushScope/PopScope, not SetRoot. " +
                "If you need '#For x in ...' to write back into the model (e.g. Order.Total = ...), " +
                "that needs a separate design decision — see CHANGES.md 'Open items'.");

        public object InvokeFunction(string name, object[] args)
        {
            if (!Functions.TryGetValue(name, out var del))
                throw new InvalidOperationException($"Function '{name}' not found");

            return del.DynamicInvoke(args);
        }
    }
}
