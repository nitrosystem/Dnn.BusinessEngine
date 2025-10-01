using System;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.Contracts
{
    public interface IModuleBuildLockService
    {
        Task<bool> TryLockAsync(Guid moduleId, int timeoutMilliseconds = 1000);
        void ReleaseLock(Guid moduleId);
    }
}
