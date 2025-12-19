using System.Threading;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Core.EngineBase
{
    public class EngineContext : IEngineContext
    {
        private readonly Dictionary<string, object> _items = new();

        public EngineExecutionPhase CurrentPhase { get;  set; }
        public string CurrentMiddleware { get;  set; }
        public CancellationTokenSource CancellationTokenSource { get; }

        public void Set<T>(string key, T value)
            => _items[key] = value;

        public T Get<T>(string key) => TryGet<T>(key, out var v) ? v : default;

        public bool TryGet<T>(string key, out T value)
        {
            if (_items.TryGetValue(key, out var obj) && obj is T t)
            {
                value = t;
                return true;
            }

            value = default;
            return false;
        }
    }
}
