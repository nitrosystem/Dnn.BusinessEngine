using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace NitroSystem.Dnn.BusinessEngine.Core.RenderTemplate
{
    public class RenderContext
    {
        private readonly Stack<Dictionary<string, object>> _scopes = new();

        public RenderContext(object model)
        {
            _scopes.Push(ObjectToDictionary(model));
        }

        public void PushScope(string name, object value)
        {
            _scopes.Push(new Dictionary<string, object> { [name] = value });
        }

        public void PopScope() => _scopes.Pop();

        public bool TryGet(string key, out object value)
        {
            foreach (var scope in _scopes)
                if (scope.TryGetValue(key, out value))
                    return true;

            value = null;
            return false;
        }

        private static Dictionary<string, object> ObjectToDictionary(object obj)
        {
            if (obj is ConcurrentDictionary<string, object> dict)
                return new Dictionary<string, object>(dict);

            return obj.GetType()
                      .GetProperties()
                      .ToDictionary(p => p.Name, p => p.GetValue(obj));
        }
    }
}