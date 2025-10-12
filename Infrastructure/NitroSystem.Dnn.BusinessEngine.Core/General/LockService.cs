using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Core.General
{
    public class LockService 
    {
        // Stores a semaphore per module to synchronize build operations.
        private static readonly Dictionary<Guid, SemaphoreSlim> _locks = new Dictionary<Guid, SemaphoreSlim>();

        // Used to synchronize access to the _locks dictionary itself.
        private static readonly object _locksAccess = new object();

        /// <summary>
        /// Tries to acquire a lock for the given module ID.
        /// Returns true if the lock was acquired within the specified timeout.
        /// </summary>
        public async Task<bool> TryLockAsync(Guid moduleId, int timeoutMilliseconds = 0)
        {
            SemaphoreSlim semaphore;

            lock (_locksAccess)
            {
                if (!_locks.TryGetValue(moduleId, out semaphore))
                {
                    semaphore = new SemaphoreSlim(1, 1);
                    _locks[moduleId] = semaphore;
                }
            }

            return await semaphore.WaitAsync(timeoutMilliseconds);
        }

        /// <summary>
        /// Releases the lock for the given module ID.
        /// </summary>
        public void ReleaseLock(Guid moduleId)
        {
            lock (_locksAccess)
            {
                if (_locks.TryGetValue(moduleId, out var semaphore))
                {
                    semaphore.Release();
                }
            }
        }
    }
}
