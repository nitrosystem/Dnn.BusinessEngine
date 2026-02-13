using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ExpressionParser.ExpressionBuilder;

namespace NitroSystem.Dnn.BusinessEngine.Core.DslEngine
{
    public sealed class DslContext : IDslContext
    {
        private readonly ConcurrentDictionary<string, object> _roots;
        private readonly Dictionary<string, Delegate> _functions = ExpressionFunctions.BuiltIn;

        public DslContext(ConcurrentDictionary<string, object> roots)
        {
            _roots = roots;
        }

        public object GetRoot(string name)
        {
            object value;
            _roots.TryGetValue(name, out value);
            //throw new InvalidOperationException("DSL root not found: " + name);

            return value;
        }

        public Type GetRootType(string name)
        {
            object value;
            if (_roots.TryGetValue(name, out value))
                return value.GetType();
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
