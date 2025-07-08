using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.IO
{
    public static class BatchExecutor
    {
        public static async Task ExecuteInBatchesAsync<T>(
            IEnumerable<T> items,
            int batchSize,
            Func<List<T>, Task> batchAction)
        {
            var batch = new List<T>(batchSize);

            foreach (var item in items)
            {
                batch.Add(item);

                if (batch.Count == batchSize)
                {
                    await batchAction(batch);
                    batch.Clear();
                }
            }

            if (batch.Count > 0)
            {
                await batchAction(batch);
            }
        }
    }
}
