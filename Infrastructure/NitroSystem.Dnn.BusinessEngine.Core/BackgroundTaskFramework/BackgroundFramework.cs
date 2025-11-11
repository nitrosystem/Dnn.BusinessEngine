using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Core.BackgroundTaskFramework.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.BackgroundTaskFramework.Models;
using NitroSystem.Dnn.BusinessEngine.Core.PushingServer;

namespace NitroSystem.Dnn.BusinessEngine.Core.BackgroundTaskFramework
{
    public class BackgroundFramework : IDisposable
    {
        private readonly NotificationServer _notificationServer;
        private readonly PriorityTaskQueue _queue = new();
        private readonly SemaphoreSlim _throttler;
        private readonly Dictionary<string, RunningTaskInfo> _runningTasks = new();
        private readonly object _lock = new();
        private readonly Timer _gcTimer;
        private readonly long _memoryThresholdBytes;
        private volatile bool _accepting = true;

        private readonly string _webSocketChannel;

        // background loop management
        private readonly CancellationTokenSource _cts = new();
        private readonly Task _dispatcherTask;

        // notification queue to avoid blocking callers
        private readonly ConcurrentQueue<NotificationItem> _notificationQueue = new();
        private readonly Task _notificationPumpTask;
        private readonly CancellationTokenSource _notificationCts = new();

        public int MaxDegreeOfParallelism { get; }

        #region NotificationItem

        private class NotificationItem
        {
            public string Channel { get; }
            public object Payload { get; }

            public NotificationItem(string channel, object payload)
            {
                Channel = channel;
                Payload = payload;
            }
        }

        #endregion

        public BackgroundFramework(IServiceProvider serviceProvider, int maxDegreeOfParallelism, long memoryThresholdBytes, string webSocketChannel)
        {
            _notificationServer = serviceProvider.GetRequiredService<NotificationServer>();

            MaxDegreeOfParallelism = Math.Max(1, maxDegreeOfParallelism);
            _throttler = new SemaphoreSlim(MaxDegreeOfParallelism, MaxDegreeOfParallelism);
            _memoryThresholdBytes = memoryThresholdBytes;
            _webSocketChannel = webSocketChannel;

            _dispatcherTask = Task.Run(() => DispatcherLoopAsync(_cts.Token), CancellationToken.None);
            _notificationPumpTask = Task.Run(() => NotificationPumpLoopAsync(_notificationCts.Token), CancellationToken.None);

            _gcTimer = new Timer(_ => MonitorGc(), null, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2));
        }

        #region Enqueue + Notifications

        private string ResolveChannel(BackgroundTaskRequest req, string overrideChannel)
        {
            if (!string.IsNullOrEmpty(overrideChannel)) return overrideChannel;
            if (!string.IsNullOrEmpty(req?.NotificationChannel)) return req.NotificationChannel;
            return _webSocketChannel;
        }

        public void Enqueue(BackgroundTaskRequest req, string channel = null)
        {
            _queue.Enqueue(req);

            var resolved = ResolveChannel(req, channel);
            if (!string.IsNullOrEmpty(resolved))
            {
                var payload = new
                {
                    type = "enqueued",
                    taskId = req.Task.TaskId,
                    name = req.Task.Name,
                    priority = req.Priority.ToString()
                };
                _notificationQueue.Enqueue(new NotificationItem(resolved, payload));
            }
        }

        private void EnqueueNotification(object payload, BackgroundTaskRequest req = null, string channel = null)
        {
            var resolved = ResolveChannel(req, channel);
            if (!string.IsNullOrEmpty(resolved))
            {
                _notificationQueue.Enqueue(new NotificationItem(resolved, payload));
            }
        }

        private async Task NotificationPumpLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (_notificationQueue.TryDequeue(out var item))
                    {
                        try
                        {
                            await Task.Run(() => _notificationServer.SendToChannel(item.Channel, item.Payload), token);
                        }
                        catch
                        {
                            // swallow errors to keep pump alive
                        }
                        continue;
                    }
                    await Task.Delay(150, token);
                }
                catch (OperationCanceledException) { break; }
                catch { await Task.Delay(500, token); }
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
                            EnqueueNotification(new { type = "progress", data = p }, req);
                        });

                        var info = new RunningTaskInfo
                        {
                            Request = req,
                            CancellationSource = cts,
                            StartedAt = DateTime.UtcNow
                        };

                        lock (_lock) _runningTasks[req.Task.TaskId] = info;

                        EnqueueNotification(new { type = "started", taskId = req.Task.TaskId, name = req.Task.Name }, req);

                        info.RunningTask = Task.Run(async () =>
                        {
                            try
                            {
                                await req.Task.RunAsync(cts.Token, progress).ConfigureAwait(false);
                                info.IsCompleted = true;
                                EnqueueNotification(new { type = "completed", taskId = req.Task.TaskId, name = req.Task.Name }, req);
                            }
                            catch (OperationCanceledException)
                            {
                                info.IsCancelled = true;
                                EnqueueNotification(new { type = "cancelled", taskId = req.Task.TaskId }, req);
                            }
                            catch (Exception ex)
                            {
                                EnqueueNotification(new { type = "error", taskId = req.Task.TaskId, message = ex.Message }, req);
                                if (req.RetryCount < 3)
                                {
                                    req.RetryCount++;
                                    Enqueue(req);
                                }
                            }
                            finally
                            {
                                lock (_lock) _runningTasks.Remove(req.Task.TaskId);
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

        #region GC Monitor

        private void MonitorGc()
        {
            try
            {
                long used = GC.GetTotalMemory(false);

                if (used > _memoryThresholdBytes)
                {
                    _accepting = false;

                    EnqueueNotification(new
                    {
                        type = "gc_alert",
                        used,
                        threshold = _memoryThresholdBytes,
                        message = "High memory usage! Pausing new tasks."
                    });

                    CancelLowPriorityRunningTasks();
                }
                else
                {
                    _accepting = true;
                }
            }
            catch { }
        }

        private void CancelLowPriorityRunningTasks()
        {
            List<string> toCancel;
            lock (_lock)
            {
                toCancel = _runningTasks.Values
                    .Where(x => x.Request.Priority <= TaskPriority.Normal && !x.IsCancelled && !x.IsCompleted)
                    .Select(x => x.Request.Task.TaskId)
                    .ToList();
            }

            foreach (var id in toCancel)
                CancelTask(id);

            if (toCancel.Count > 0)
            {
                EnqueueNotification(new { type = "gc_cleanup", cancelledTasks = toCancel });
            }
        }

        #endregion

        #region Control APIs

        public void CancelTask(string taskId)
        {
            lock (_lock)
            {
                if (_runningTasks.TryGetValue(taskId, out var info))
                {
                    try { info.CancellationSource.Cancel(); } catch { }
                    EnqueueNotification(new { type = "manual_cancel", taskId });
                }
            }
        }

        public void CancelByPriority(TaskPriority priority)
        {
            List<string> targets;
            lock (_lock)
            {
                targets = _runningTasks.Values
                    .Where(x => x.Request.Priority == priority && !x.IsCancelled && !x.IsCompleted)
                    .Select(x => x.Request.Task.TaskId)
                    .ToList();
            }

            foreach (var id in targets)
                CancelTask(id);
        }

        public void Pause() => _accepting = false;
        public void Resume() => _accepting = true;

        public void DrainQueue()
        {
            EnqueueNotification(new { type = "queue_drained", count = _queue.Count });
            while (_queue.TryDequeue(out _)) { }
        }

        public IReadOnlyCollection<RunningTaskInfo> GetRunningTasks()
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
                _gcTimer?.Dispose();

                _cts.Cancel();
                try { _dispatcherTask?.Wait(TimeSpan.FromSeconds(5)); } catch { }

                _notificationCts.Cancel();
                try { _notificationPumpTask?.Wait(TimeSpan.FromSeconds(5)); } catch { }

                List<RunningTaskInfo> running;
                lock (_lock) running = _runningTasks.Values.ToList();
                foreach (var r in running)
                {
                    try { r.CancellationSource.Cancel(); } catch { }
                }

                _throttler?.Dispose();
                _cts.Dispose();
                _notificationCts.Dispose();
            }
            catch { }
        }

        #endregion
    }
}
