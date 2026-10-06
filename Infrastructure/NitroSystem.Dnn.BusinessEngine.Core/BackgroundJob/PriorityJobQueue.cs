using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NitroSystem.Dnn.BusinessEngine.Core.BackgroundJob.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.BackgroundJob.Models;

namespace NitroSystem.Dnn.BusinessEngine.Core.BackgroundJob
{
    public class PriorityJobQueue
    {
        private readonly SortedDictionary<int, ConcurrentQueue<JobRequest>> _queues
            = new SortedDictionary<int, ConcurrentQueue<JobRequest>>();

        public PriorityJobQueue()
        {
            foreach (int p in Enum.GetValues(typeof(JobPriority)))
                _queues[p] = new ConcurrentQueue<JobRequest>();
        }

        public void Enqueue(JobRequest req)
        {
            int key = (int)req.Priority;
            if (req.ForceRun) // urgent: push to highest priority queue head — we'll enqueue in Critical
            {
                _queues[(int)JobPriority.Critical].Enqueue(req);
                return;
            }
            _queues[key].Enqueue(req);
        }

        public bool TryDequeue(out JobRequest req)
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
