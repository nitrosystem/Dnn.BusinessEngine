using System;
using System.Threading;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Shared.Helpers
{
    public static class AsyncHelper
    {
        /// <summary>
        /// Executes an async Task<T> method synchronously and returns its result.
        /// </summary>
        public static T RunSync<T>(Func<Task<T>> task)
        {
            return Task.Run(task).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Executes an async Task method synchronously (no return value).
        /// </summary>
        public static void RunSync(Func<Task> task)
        {
            Task.Run(task).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Executes an async Task<T> method synchronously with cancellation support.
        /// </summary>
        public static T RunSync<T>(Func<CancellationToken, Task<T>> task, CancellationToken ct)
        {
            return Task.Run(() => task(ct), ct).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
