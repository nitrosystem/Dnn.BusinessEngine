using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections.Concurrent;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Core.BackgroundTaskFramework.Models;
using NitroSystem.Dnn.BusinessEngine.Core.Workflow.Extensions;
using NitroSystem.Dnn.BusinessEngine.Core.Workflow.Models;
using NitroSystem.Dnn.BusinessEngine.Core.BackgroundTaskFramework;
using NitroSystem.Dnn.BusinessEngine.Core.BackgroundTaskFramework.Enums;
using DotNetNuke.Services.Search;

namespace NitroSystem.Dnn.BusinessEngine.Core.Workflow
{
    public class WorkflowEventManager : IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ResourceProfiler _resourceProfiler;
        private readonly BackgroundFramework _backgroundFramework;

        // In-memory maps/queues
        private readonly ConcurrentDictionary<string, TaskInfo> _tasks = new ConcurrentDictionary<string, TaskInfo>();
        private readonly ConcurrentQueue<TaskInfo> _pendingSaves = new ConcurrentQueue<TaskInfo>();

        // batching config
        private readonly int _batchSize;
        private readonly TimeSpan _flushInterval;
        private readonly Timer _flushTimer;

        // simple lock to avoid concurrent flushes
        private int _isFlushing;

        public string TaskId { get; }
        public string Name { get; }

        public WorkflowEventManager(
            IServiceProvider serviceProvider,
            ResourceProfiler resourceProfiler,
            BackgroundFramework backgroundFramework,
            int batchSize = 50,
            TimeSpan? flushInterval = null)
        {
            _serviceProvider = serviceProvider;
            _resourceProfiler = resourceProfiler ?? throw new ArgumentNullException(nameof(resourceProfiler));
            _backgroundFramework = backgroundFramework ?? throw new ArgumentNullException(nameof(backgroundFramework));

            _batchSize = Math.Max(1, batchSize);
            _flushInterval = flushInterval ?? TimeSpan.FromSeconds(5);

            // setup timer to periodically try flushing
            _flushTimer = new Timer(async _ => await TimerFlushCallbackAsync(), null, _flushInterval, _flushInterval);
        }

        public async Task<IReadOnlyList<object>> ExecuteTasksAsync<T>(
            string workflowName,
            string eventName,
            string stepName,
            int userId,
            bool continueOnError,
            params LambdaExpression[] expressions)
        {
            var list = new List<object>();

            foreach (var expr in expressions)
            {
                var bodyType = expr.ReturnType;

                // -- Task<T>
                if (bodyType.IsGenericType && bodyType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    list.Add(await ExecuteTaskAsync<T>(workflowName, eventName, stepName, userId, continueOnError, false, expr));
                }
                // -- Task
                else if (bodyType == typeof(Task))
                {
                    list.Add(await ExecuteTaskAsync<T>(workflowName, eventName, stepName, userId, continueOnError, true, expr));
                }
            }

            return list;
        }

        // public API: execute set of tasks (tasks are expressions returning Task<T>)
        private async Task<object> ExecuteTaskAsync<T>(
            string workflowName,
            string eventName,
            string stepName,
            int userId,
            bool continueOnError,
            bool isvoid,
            LambdaExpression expression)
        {
            var results = new List<object>();

            // run and profile the tasks (ResourceProfiler.ExecuteTasksAsync should return MethodProfileResult<T> list)
            var taskResult = await _resourceProfiler.ExecuteTasksAsync<T>(isvoid, expression);
            // build TaskInfo (adapt to your actual TaskInfo fields)
            var task = new TaskInfo
            {
                WorkflowName = workflowName,
                EventName = eventName,
                StepName = stepName,
                TaskName = taskResult.Name,
                UserId = userId,
                StartTime = taskResult.StartTime,
                EndTime = taskResult.EndTime,
                Exception = taskResult.Exception,
                IsError = taskResult.IsError,
                ResourceUsage = taskResult.ResourceUsage
            };

            // add to in-memory dictionary (keyed by workflowName as you used before)
            _tasks.TryAdd(workflowName, task);

            // enqueue for background persistence
            _pendingSaves.Enqueue(task);

            // if we reached batch size, try an immediate flush (fire-and-forget)
            if (_pendingSaves.Count >= _batchSize)
            {
                _ = Task.Run(() => TriggerFlushIfNeededAsync());
            }

            if (taskResult.IsError && !continueOnError)
                throw taskResult.Exception;

            return taskResult.Result;
        }

        // Timer callback wrapper to avoid exception swallowing and concurrency
        private async Task TimerFlushCallbackAsync()
        {
            try
            {
                await TriggerFlushIfNeededAsync();
            }
            catch
            {
                // swallow - timer loop should stay alive
            }
        }

        // Ensure only one flush runs concurrently
        private async Task TriggerFlushIfNeededAsync()
        {
            // Nothing to do?
            if (_pendingSaves.IsEmpty)
                return;

            // prevent concurrent flush
            if (Interlocked.Exchange(ref _isFlushing, 1) == 1)
                return;

            try
            {
                // process ALL pending items, but in batches
                while (!_pendingSaves.IsEmpty)
                {
                    var batch = new List<TaskInfo>(_batchSize);

                    // If enough items → take batchSize
                    // If not enough → take the entire remaining buffer
                    while (batch.Count < _batchSize && _pendingSaves.TryDequeue(out var item))
                    {
                        batch.Add(item);
                    }

                    if (batch.Count == 0)
                        break;

                    // NOW flush this batch (full or partial)
                    var job = new SaveTaskBatchJob(_serviceProvider, batch);
                    var req = new BackgroundTaskRequest(job, TaskPriority.Normal);
                    _backgroundFramework.Enqueue(req);

                    await Task.Yield();
                }
            }
            finally
            {
                Volatile.Write(ref _isFlushing, 0);
            }
        }

        // --- IBackgroundTask implementation (if you want this manager to itself be runnable as a background task)
        public async Task RunAsync(CancellationToken token, IProgress<ProgressInfo> progress)
        {
            // Optionally, keep flushing while running (if you want it to be actively flushing)
            while (!token.IsCancellationRequested)
            {
                await TriggerFlushIfNeededAsync();
                await Task.Delay(_flushInterval, token);
            }
        }

        // Public helper to view pending items count
        public int PendingSavesCount => _pendingSaves.Count;

        // Dispose: stop timer and enqueue any remaining batches for persistence
        public void Dispose()
        {
            try
            {
                _flushTimer?.Dispose();
            }
            catch { }

            // Fire-and-forget flush of remaining items (do not block Dispose)
            _ = Task.Run(async () =>
            {
                try
                {
                    // keep flushing until queue is empty
                    while (!_pendingSaves.IsEmpty)
                    {
                        await TriggerFlushIfNeededAsync();
                        // small delay to allow background framework to pick up jobs
                        await Task.Delay(100);
                    }
                }
                catch { /* swallow */ }
            });
        }
    }
}
