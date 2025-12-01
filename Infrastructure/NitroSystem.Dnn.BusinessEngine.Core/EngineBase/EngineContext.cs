using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;
using NitroSystem.Dnn.BusinessEngine.Core.Workflow;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase;

namespace NitroSystem.Dnn.BusinessEngine.Core.EngineBase
{
    public class EngineContext : IEngineContext
    {
        private readonly ConcurrentDictionary<string, object> _items = new ConcurrentDictionary<string, object>();

        public virtual CancellationToken CancellationToken { get; }
        public IDictionary<string, object> Items { get; } = new Dictionary<string, object>();
        public WorkflowManager EventManager { get; internal set; }

        public EngineContext(CancellationToken cancellationToken = default)
        {
            CancellationToken = cancellationToken;
        }

        public void Set<T>(string key, T value) => _items[key] = value;
    
        public bool TryGet<T>(string key, out T value)
        {
            value = default;

            if (_items.TryGetValue(key, out var o) && o is T t)
            {
                value = t;
                return true;
            }

            return false;
        }
      
        public T Get<T>(string key) => TryGet<T>(key, out var v) ? v : default;
    }
}
