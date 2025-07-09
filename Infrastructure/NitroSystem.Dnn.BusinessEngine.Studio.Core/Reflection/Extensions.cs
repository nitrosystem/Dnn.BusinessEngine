using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.Reflection
{
    public static class Extensions
    {
        public static async Task ForEachAsync<T>(
            this IEnumerable<T> source,
            Func<T, Task> action,
            int maxDegreeOfParallelism = 4)
        {
            var throttler = new SemaphoreSlim(maxDegreeOfParallelism);

            var tasks = source.Select(async item =>
            {
                await throttler.WaitAsync();
                try
                {
                    await action(item);
                }
                finally
                {
                    throttler.Release();
                }
            });

            await Task.WhenAll(tasks);
        }
    }
}
