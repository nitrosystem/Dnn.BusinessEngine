using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Core.EvenFramework.Models;

namespace NitroSystem.Dnn.BusinessEngine.Core.EvenFramework
{
    public class EventContext
    {
        // Flexible bag for passing data between nodes
        public ConcurrentDictionary<string, object> Items { get; } = new ConcurrentDictionary<string, object>();
        public Guid Id { get; }
        public string EventName { get; }
        public DateTime StartTime { get; private set; }
        public DateTime? EndTime { get; private set; }
        public Exception Exception { get; private set; }
        public List<EventNode> Nodes { get; } = new();

        public EventContext(string eventName)
        {
            EventName = eventName;
        }

        public EventNode AddNode(string name)
        {
            var node = new EventNode(name);
            Nodes.Add(node);
            return node;
        }

        public void Start() => StartTime = DateTime.Now;
        public void Complete() => EndTime = DateTime.Now;
        public void Fail(Exception ex)
        {
            Exception = ex;
            EndTime = DateTime.Now;
        }
    }
}
