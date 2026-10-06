using System;
using System.Collections.Concurrent;
using System.Reflection;
using NitroSystem.Dnn.BusinessEngine.Core.ExpressionEngine.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ExpressionEngine.Functions;

namespace NitroSystem.Dnn.BusinessEngine.Core.ExpressionEngine
{
    public class ExpressionContext : IExpressionContext
    {
        private readonly ConcurrentDictionary<string, object> _roots;
        private readonly ConcurrentDictionary<string, Delegate> _functions = ExpressionFunctions.BuiltIn;

        public ExpressionContext(ConcurrentDictionary<string, object> roots)
        {
            _roots = roots;
        }

        public object GetRoot(string name)
        {
            _roots.TryGetValue(name, out var value);
            return value;
        }

        public Type GetRootType(string name)
        {
            object value;
            if (_roots.TryGetValue(name, out value))
                return value?.GetType() ?? typeof(object);
            else
                return typeof(object);
        }

        public void SetRoot(string name, object value)
        {
            if (!_roots.ContainsKey(name))
                throw new InvalidOperationException("DSL root not found: " + name);

            _roots[name] = value;
        }

        public object InvokeFunction(string name, object[] args)
        {
            if (!_functions.TryGetValue(name, out var del))
                throw new InvalidOperationException($"Function '{name}' not found");

            try
            {
                return del.DynamicInvoke(args);
            }
            catch (TargetParameterCountException)
            {
                throw new InvalidOperationException(
                    $"Invalid argument count for function '{name}'");
            }
        }
    }
}
