using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.General
{
    public static class BatchExecutor
    {
        public static async Task ExecuteInBatchesAsync<T>(
            IEnumerable<T> items,
            int batchSize,
            Func<IList<T>, Task> batchAction,
            CancellationToken cancellationToken = default)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (batchSize <= 0) throw new ArgumentOutOfRangeException(nameof(batchSize));
            if (batchAction == null) throw new ArgumentNullException(nameof(batchAction));

            var batch = new List<T>(batchSize);

            foreach (var item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();
                batch.Add(item);

                if (batch.Count == batchSize)
                {
                    await ExecuteSingleBatchAsync(batchAction, batch, cancellationToken).ConfigureAwait(false);
                    batch = new List<T>(batchSize);
                }
            }

            if (batch.Count > 0)
            {
                await ExecuteSingleBatchAsync(batchAction, batch, cancellationToken).ConfigureAwait(false);
            }
        }

        private static async Task ExecuteSingleBatchAsync<T>(
            Func<IList<T>, Task> batchAction,
            IList<T> batch,
            CancellationToken cancellationToken)
        {
            try
            {
                // If caller cancels inside batchAction, OperationCanceledException bubbles up.
                await batchAction(batch).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Optional: لاگ یا بسته‌بندی خطا. اینجا یک InvalidOperationException می‌دهیم
                throw new InvalidOperationException("Batch execution failed.", ex);
            }
        }
    }
}
