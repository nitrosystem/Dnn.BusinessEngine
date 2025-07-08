using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Contracts
{
    public interface IModuleBuildLockService
    {
        Task<bool> TryLockAsync(Guid moduleId, int timeoutMilliseconds = 1000);
        void ReleaseLock(Guid moduleId);
    }
}
