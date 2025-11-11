using NitroSystem.Dnn.BusinessEngine.Core.BackgroundTaskFramework.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.BackgroundTaskFramework.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.BackgroundTaskFramework
{
    public class PriorityTaskQueue
    {
        private readonly SortedDictionary<int, ConcurrentQueue<BackgroundTaskRequest>> _queues
            = new SortedDictionary<int, ConcurrentQueue<BackgroundTaskRequest>>();
        private readonly object _locker = new object();

        public PriorityTaskQueue()
        {
            foreach (int p in Enum.GetValues(typeof(TaskPriority)))
                _queues[p] = new ConcurrentQueue<BackgroundTaskRequest>();
        }

        public void Enqueue(BackgroundTaskRequest req)
        {
            int key = (int)req.Priority;
            if (req.ForceRun) // urgent: push to highest priority queue head — we'll enqueue in Critical
            {
                _queues[(int)TaskPriority.Critical].Enqueue(req);
                return;
            }
            _queues[key].Enqueue(req);
        }

        public bool TryDequeue(out BackgroundTaskRequest req)
        {
            foreach (var kv in _queues.OrderByDescending(k => k.Key))
            {
                if (kv.Value.TryDequeue(out req)) return true;
            }
            req = null;
            return false;
        }

        public int Count => _queues.Sum(q => q.Value.Count);
    }
}
