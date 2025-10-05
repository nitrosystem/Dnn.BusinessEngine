using System;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.BuildModuleEngine.Contracts
{
    public interface IModuleBuildLockService
    {
        Task<bool> TryLockAsync(Guid moduleId, int timeoutMilliseconds = 1000);
        void ReleaseLock(Guid moduleId);
    }
}
