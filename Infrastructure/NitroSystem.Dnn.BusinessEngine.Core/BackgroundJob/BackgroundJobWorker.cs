using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Core.BackgroundJob.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.BackgroundJob.Models;

namespace NitroSystem.Dnn.BusinessEngine.Core.BackgroundJob
{
    public sealed class BackgroundJobWorker : IDisposable
    {
        private readonly JobQueue _queue = new();
        private readonly Dictionary<string, RunningJobInfo> _running = new();

        private readonly SemaphoreSlim _throttler;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly object _lock = new();

        private readonly CancellationTokenSource _cts = new();
        private readonly Task _dispatcherTask;

        private volatile bool _accepting = true;

        public int MaxDegreeOfParallelism { get; }

        // Events
        public event Action<JobContext> JobEnqueued;
        public event Action<JobContext> JobStarted;
        public event Action<JobContext> JobCompleted;
        public event Action<JobContext, Exception> JobFailed;
        public event Action<JobContext> JobCancelled;

        public BackgroundJobWorker(
            IServiceScopeFactory scopeFactory,
            int maxDegreeOfParallelism)
        {
            _scopeFactory = scopeFactory;
            MaxDegreeOfParallelism = Math.Max(1, maxDegreeOfParallelism);

            _throttler = new SemaphoreSlim(MaxDegreeOfParallelism, MaxDegreeOfParallelism);
            _dispatcherTask = Task.Run(() => DispatcherLoopAsync(_cts.Token));
        }

        #region Enqueue

        public void Enqueue(JobContext context)
        {
            _queue.Enqueue(context);
            JobEnqueued?.Invoke(context);
        }

        #endregion

        #region Dispatcher

        private async Task DispatcherLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (!_accepting)
                    {
                        await Task.Delay(200, token);
                        continue;
                    }

                    if (!_queue.TryDequeue(out var context))
                    {
                        await Task.Delay(150, token);
                        continue;
                    }

                    await _throttler.WaitAsync(token);

                    var jobCts = CancellationTokenSource.CreateLinkedTokenSource(token);
                    var running = new RunningJobInfo
                    {
                        Request = context,
                        StartedAt = DateTime.UtcNow,
                        CancellationSource = jobCts
                    };

                    lock (_lock)
                        _running[context.JobId] = running;

                    JobStarted?.Invoke(context);

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            using var scope = _scopeFactory.CreateScope();

                            var job = (IJob)scope.ServiceProvider.GetRequiredService(context.JobType);

                            await job.RunAsync(context, jobCts.Token);

                            running.IsCompleted = true;
                            JobCompleted?.Invoke(context);
                        }
                        catch (OperationCanceledException)
                        {
                            running.IsCancelled = true;
                            JobCancelled?.Invoke(context);
                        }
                        catch (Exception ex)
                        {
                            JobFailed?.Invoke(context, ex);
                        }
                        finally
                        {
                            lock (_lock)
                                _running.Remove(context.JobId);

                            _throttler.Release();
                        }

                    }, CancellationToken.None);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch
                {
                    await Task.Delay(500, token);
                }
            }
        }

        #endregion

        #region Control

        public void CancelJob(string jobId)
        {
            lock (_lock)
            {
                if (_running.TryGetValue(jobId, out var job))
                    job.CancellationSource.Cancel();
            }
        }

        public IReadOnlyCollection<RunningJobInfo> GetRunningJobs()
        {
            lock (_lock)
                return _running.Values.ToList();
        }

        public void Pause() => _accepting = false;
        public void Resume() => _accepting = true;

        #endregion

        #region Dispose

        public void Dispose()
        {
            _cts.Cancel();
            _dispatcherTask.Wait(TimeSpan.FromSeconds(5));

            lock (_lock)
            {
                foreach (var job in _running.Values)
                    job.CancellationSource.Cancel();
            }

            _throttler.Dispose();
            _cts.Dispose();
        }

        #endregion
    }
}
