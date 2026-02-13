using System;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Core.BackgroundJob
{
    public sealed class JobContext
    {
        private readonly Dictionary<string, object> _items = new();

        public string JobId { get; set; } = Guid.NewGuid().ToString("N");
        public Type JobType { get; set; }
        public DateTime EnqueuedAt { get; set; }

        public void Set<T>(string key, T value)
        {
            _items[key] = value!;
        }

        public T Get<T>(string key)
        {
            if (!_items.TryGetValue(key, out var value))
                throw new Exception($"Export context key not found: {key}");

            return (T)value;
        }

        public bool TryGet<T>(string key, out T value)
        {
            if (!_items.TryGetValue(key, out var v))
            {
                value = default;
                return false;
            }

            value = (T)v;
            return false;
        }
    }
}
