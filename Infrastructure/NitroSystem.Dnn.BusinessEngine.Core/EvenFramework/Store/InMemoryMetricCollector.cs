using NitroSystem.Dnn.BusinessEngine.Core.EvenFramework.Contracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.EvenFramework.Store
{
    public class InMemoryMetricCollector : IMetricCollector
    {
        private readonly ConcurrentDictionary<(Guid, string), ConcurrentDictionary<string, double>> _store
            = new();

        public void RecordNodeMetric(Guid eventId, string nodeName, string metricKey, double value)
        {
            var key = (eventId, nodeName ?? "");
            var map = _store.GetOrAdd(key, _ => new ConcurrentDictionary<string, double>());
            map[metricKey] = value;
        }

        public double? GetMetric(Guid eventId, string nodeName, string metricKey)
        {
            var key = (eventId, nodeName ?? "");
            if (_store.TryGetValue(key, out var map) && map.TryGetValue(metricKey, out var v))
                return v;
            return null;
        }

        public void ClearForEvent(Guid eventId)
        {
            var keys = _store.Keys.Where(k => k.Item1 == eventId).ToList();
            foreach (var k in keys) _store.TryRemove(k, out _);
        }
    }
}
