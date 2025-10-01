using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule
{
    public class ModuleBuildQueueManager
    {
        private static readonly Dictionary<Guid, Queue<Func<Task>>> _queues = new Dictionary<Guid, Queue<Func<Task>>>();
        private static readonly Dictionary<Guid, bool> _isProcessing = new Dictionary<Guid, bool>();
        private static readonly object _syncRoot = new object();

        public void Enqueue(Guid moduleId, Func<Task> buildTask)
        {
            lock (_syncRoot)
            {
                if (!_queues.ContainsKey(moduleId))
                    _queues[moduleId] = new Queue<Func<Task>>();

                _queues[moduleId].Enqueue(buildTask);

                if (!_isProcessing.ContainsKey(moduleId) || !_isProcessing[moduleId])
                {
                    _isProcessing[moduleId] = true;
                    _ = ProcessQueueAsync(moduleId);
                }
            }
        }

        private async Task ProcessQueueAsync(Guid moduleId)
        {
            while (true)
            {
                Func<Task> taskToRun = null;

                lock (_syncRoot)
                {
                    if (!_queues.TryGetValue(moduleId, out var queue) || queue.Count == 0)
                    {
                        _isProcessing[moduleId] = false;
                        return;
                    }

                    taskToRun = queue.Dequeue();
                }

                try
                {
                    await taskToRun();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"🔥 Error in module build task: {ex.Message}");
                }
            }
        }
    }
}
