using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Core.BackgroundJob.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.BackgroundJob.Events;
using NitroSystem.Dnn.BusinessEngine.Core.BackgroundJob.Models;

namespace NitroSystem.Dnn.BusinessEngine.Core.BackgroundJob
{
    public class BackgroundJobWorker : IDisposable
    {
        private readonly PriorityJobQueue _queue = new();
        private readonly Dictionary<string, RunningJobInfo> _runningTasks = new();
        private readonly SemaphoreSlim _throttler;
        private readonly object _lock = new();
        private volatile bool _accepting = true;

        // background loop management
        private readonly CancellationTokenSource _cts = new();
        private readonly Task _dispatcherJob;

        public event BackgroundJobProgressHandler OnProgress;
        public event BackgroundJobErrorHandler OnError;

        public int MaxDegreeOfParallelism { get; }

        public BackgroundJobWorker(int maxDegreeOfParallelism)
        {
            MaxDegreeOfParallelism = Math.Max(1, maxDegreeOfParallelism);
            _throttler = new SemaphoreSlim(MaxDegreeOfParallelism, MaxDegreeOfParallelism);

            _dispatcherJob = Task.Run(() => DispatcherLoopAsync(_cts.Token), CancellationToken.None);
        }

        #region Enqueue + Notifications


        public void Enqueue(JobRequest req, string channel = null)
        {
            _queue.Enqueue(req);

            RaiseEvent("enqueued",
                new
                {
                    jobId = req.Job.JobId,
                    name = req.Job.Name,
                    priority = req.Priority.ToString()
                }
            );
        }

        private void RaiseEvent(string type, object payload = null)
        {
            switch (type)
            {
                case "progress":
                    OnProgress?.Invoke(payload);
                    break;
                case "error":
                    OnError?.Invoke(payload);
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region Dispatcher Loop

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

                    if (_queue.TryDequeue(out var req))
                    {
                        await _throttler.WaitAsync(token);

                        var cts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token);
                        var progress = new Progress<ProgressInfo>(p =>
                        {
                            RaiseEvent("progress", p);
                        });

                        var info = new RunningJobInfo
                        {
                            Request = req,
                            CancellationSource = cts,
                            StartedAt = DateTime.UtcNow
                        };

                        lock (_lock) _runningTasks[req.Job.JobId] = info;

                        RaiseEvent("started", new { jobId = req.Job.JobId, name = req.Job.Name });

                        info.RunningJob = Task.Run(async () =>
                        {
                            try
                            {
                                await req.Job.RunAsync(cts.Token).ConfigureAwait(false);
                                info.IsCompleted = true;
                                RaiseEvent("completed", new { jobId = req.Job.JobId, name = req.Job.Name });
                            }
                            catch (OperationCanceledException)
                            {
                                info.IsCancelled = true;
                                RaiseEvent("cancelled", new { jobId = req.Job.JobId });
                            }
                            catch (Exception ex)
                            {
                                RaiseEvent("error", new { jobId = req.Job.JobId, message = ex.Message });
                                if (req.RetryCount > 0)
                                {
                                    req.RetryCount++;
                                    Enqueue(req);
                                }
                            }
                            finally
                            {
                                lock (_lock) _runningTasks.Remove(req.Job.JobId);
                                try { _throttler.Release(); } catch { }
                            }
                        }, CancellationToken.None);

                        continue;
                    }
                    else
                    {
                        await Task.Delay(150, token);
                    }
                }
                catch (OperationCanceledException) { break; }
                catch { await Task.Delay(500, token); }
            }
        }

        #endregion

        #region Control APIs

        public void CancelJob(string jobId)
        {
            lock (_lock)
            {
                if (_runningTasks.TryGetValue(jobId, out var info))
                {
                    try { info.CancellationSource.Cancel(); } catch { }
                    RaiseEvent("manual_cancel", new { jobId });
                }
            }
        }

        public void CancelByPriority(JobPriority priority)
        {
            List<string> targets;
            lock (_lock)
            {
                targets = _runningTasks.Values
                    .Where(x => x.Request.Priority == priority && !x.IsCancelled && !x.IsCompleted)
                    .Select(x => x.Request.Job.JobId)
                    .ToList();
            }

            foreach (var id in targets)
                CancelJob(id);
        }

        public void Pause() => _accepting = false;
        public void Resume() => _accepting = true;

        public void DrainQueue()
        {
            RaiseEvent("queue_drained", new { count = _queue.Count });
            while (_queue.TryDequeue(out _)) { }
        }

        public IReadOnlyCollection<RunningJobInfo> GetRunningTasks()
        {
            lock (_lock)
                return _runningTasks.Values.ToList();
        }

        #endregion

        #region Dispose

        public void Dispose()
        {
            try
            {
                _cts.Cancel();
                try { _dispatcherJob?.Wait(TimeSpan.FromSeconds(5)); } catch { }

                List<RunningJobInfo> running;
                lock (_lock) running = _runningTasks.Values.ToList();
                foreach (var r in running)
                {
                    try { r.CancellationSource.Cancel(); } catch { }
                }

                _throttler?.Dispose();
                _cts.Dispose();
            }
            catch { }
        }

        #endregion
    }
}
