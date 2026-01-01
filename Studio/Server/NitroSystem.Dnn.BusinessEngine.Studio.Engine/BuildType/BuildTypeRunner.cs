using System;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.TypeBuilder
{
    public class BuildTypeRunner
    {
        private readonly IEngineRunner _engineRunner;
        private readonly LockService _lockService;

        public BuildTypeRunner(IEngineRunner engineRunner, LockService lockService)
        {
            _engineRunner = engineRunner;
            _lockService = lockService;
        }

        public async Task<BuildTypeResponse> RunAsync(BuildTypeRequest request)
        {
            var lockId = request.ScenarioName + request.ModelName;

            var lockAcquired = await _lockService.TryLockAsync(lockId);
            if (!lockAcquired)
            {
                throw new InvalidOperationException("This type builder is currently being build. Please try again in a few moments..");
            }

            try
            {
                var engine = new BuildTypeEngine();
                var response = await _engineRunner.RunAsync(engine, request);
                return response;
            }
            finally
            {
                _lockService.ReleaseLock(lockId);
            }
        }
    }
}
