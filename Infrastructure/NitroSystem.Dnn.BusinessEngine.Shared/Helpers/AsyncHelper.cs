using System;
using System.Threading;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Shared.Helpers
{
    public static class AsyncHelper
    {
        /// <summary>
        /// Executes an async Task method synchronously, safely for Web Forms SSR.
        /// ⚠️ Caller must pass all required context as parameters; the async method must not use Thread-local context.
        /// </summary>
        public static T RunSync<T>(Func<Task<T>> taskFactory)
        {
            if (taskFactory == null) throw new ArgumentNullException(nameof(taskFactory));

            try
            {
                // ConfigureAwait(false) ensures no deadlock on WebForm's sync context
                return Task.Run(taskFactory).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (AggregateException ex)
            {
                // Unwrap AggregateException and rethrow original exception
                if (ex.InnerExceptions.Count == 1)
                    throw ex.InnerException!;
                throw;
            }
        }

        /// <summary>
        /// Executes an async Task (no return value) synchronously
        /// </summary>
        public static void RunSync(Func<Task> taskFactory)
        {
            RunSync<object>(async () =>
            {
                await taskFactory().ConfigureAwait(false);
                return null!;
            });
        }
    }
}
