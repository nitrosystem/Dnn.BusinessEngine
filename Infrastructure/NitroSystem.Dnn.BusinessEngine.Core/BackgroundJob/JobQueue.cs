using System.Collections.Concurrent;

namespace NitroSystem.Dnn.BusinessEngine.Core.BackgroundJob
{
    public sealed class JobQueue
    {
        private readonly ConcurrentQueue<JobContext> _queue = new();

        public void Enqueue(JobContext job) => _queue.Enqueue(job);

        public bool TryDequeue(out JobContext job) => _queue.TryDequeue(out job);

        public int Count => _queue.Count;
    }

}
