using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using NitroSystem.Dnn.BusinessEngine.Core.DSLEngine.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Core.DSLEngine
{
    public sealed class DslContext : IDslContext
    {
        private readonly ConcurrentDictionary<string, object> _roots;

        public DslContext(ConcurrentDictionary<string, object> roots)
        {
            _roots = roots;
        }

        public object GetRoot(string name)
        {
            object value;
            if (!_roots.TryGetValue(name, out value))
                throw new InvalidOperationException("DSL root not found: " + name);

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
    }
}
