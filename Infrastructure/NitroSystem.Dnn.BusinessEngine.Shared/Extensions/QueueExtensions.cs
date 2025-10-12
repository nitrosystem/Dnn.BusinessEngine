using System;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Shared.Extensions
{
    public static class QueueExtensions
    {
        /// <summary>
        /// Attempts to dequeue an item from the queue. Returns true if successful.
        /// </summary>
        public static bool TryDequeue<T>(this Queue<T> queue, out T item)
        {
            if (queue == null)
                throw new ArgumentNullException(nameof(queue));

            if (queue.Count > 0)
            {
                item = queue.Dequeue();
                return true;
            }

            item = default;
            return false;
        }
    }
}
