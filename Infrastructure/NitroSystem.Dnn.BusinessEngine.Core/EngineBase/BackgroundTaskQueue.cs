using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Core.EngineBase
{
    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly ConcurrentQueue<Func<CancellationToken, Task>> _items = new();
        private readonly SemaphoreSlim _signal = new(0);

        public void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem)
        {
            if (workItem == null) throw new ArgumentNullException(nameof(workItem));
            _items.Enqueue(workItem);
            _signal.Release();
        }

        public async Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            _items.TryDequeue(out var workItem);
            return workItem;
        }
    }
}
