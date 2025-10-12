using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.General
{
    public static class ParallelBatchExecutor
    {
        public static async Task ExecuteInParallelBatchesAsync<T>(
            IEnumerable<T> items,
            int batchSize,
            int maxDegreeOfParallelism,
            Func<IList<T>, Task> batchAction,
            CancellationToken cancellationToken = default)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (batchAction == null) throw new ArgumentNullException(nameof(batchAction));
            if (batchSize <= 0) throw new ArgumentOutOfRangeException(nameof(batchSize));
            if (maxDegreeOfParallelism <= 0) throw new ArgumentOutOfRangeException(nameof(maxDegreeOfParallelism));

            // Lazy evaluation رو از بین می‌بریم
            var itemList = items as IList<T> ?? items.ToList();
            if (itemList.Count == 0)
                return;

            // تقسیم لیست به batch‌های جداگانه
            var batches = itemList
                .Select((item, index) => new { item, index })
                .GroupBy(x => x.index / batchSize)
                .Select(g => g.Select(x => x.item).ToList())
                .ToList();

            using var semaphore = new SemaphoreSlim(maxDegreeOfParallelism);
            var tasks = new List<Task>();

            foreach (var batch in batches)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

                var task = Task.Run(async () =>
                {
                    try
                    {
                        await batchAction(batch).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException("Batch execution failed.", ex);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }, cancellationToken);

                tasks.Add(task);
            }

            // منتظر اتمام همه batchها
            try
            {
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Exception aggregation
                var allExceptions = tasks
                    .Where(t => t.IsFaulted)
                    .SelectMany(t => t.Exception!.InnerExceptions)
                    .ToList();

                throw new AggregateException("One or more batch executions failed.", allExceptions);
            }
        }
    }
}