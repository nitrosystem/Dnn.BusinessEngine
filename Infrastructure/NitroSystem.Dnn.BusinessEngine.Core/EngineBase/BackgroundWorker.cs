using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase.Contracts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.EngineBase
{
    // مصرف‌کننده ساده (در .NET Core می‌شه IHostedService ساخت؛ در .NET 4.8 از Task.Run استفاده کن)
    public class BackgroundWorker
    {
        private readonly IBackgroundTaskQueue _queue;
        private readonly CancellationToken _token;

        public BackgroundWorker(IBackgroundTaskQueue queue, CancellationToken token)
        {
            _queue = queue;
            _token = token;
            Task.Run(WorkerLoop);
        }

        private async Task WorkerLoop()
        {
            while (!_token.IsCancellationRequested)
            {
                try
                {
                    var work = await _queue.DequeueAsync(_token);
                    await work(_token);
                }
                catch (OperationCanceledException) { break; }
                catch { /* لاگ کن */ }
            }
        }
    }
}
