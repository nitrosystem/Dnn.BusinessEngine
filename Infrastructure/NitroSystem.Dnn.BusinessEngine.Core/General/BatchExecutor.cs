using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

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

            // Important: Force evaluation (In case of being Enumerable and lazy)
            var itemList = items as IList<T> ?? items.ToList();
            if (itemList.Count == 0)
                return;

            var batch = new List<T>(batchSize);

            foreach (var item in itemList)
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
                await batchAction(batch).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
        }
    }
}
