using System;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildType;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildType.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.TypeBuilder
{
    public class BuildTypeRunner: IBuildTypeRunner
    {
        private readonly IEngineRunner _engineRunner;
        private readonly LockService _lockService;
        private readonly BuildTypeEngine _engine;

        public BuildTypeRunner(IEngineRunner engineRunner, LockService lockService, BuildTypeEngine engine)
        {
            _engineRunner = engineRunner;
            _lockService = lockService;
            _engine = engine;
        }

        public async Task<BuildTypeResponse> RunAsync(BuildTypeRequest request)
        {
            var lockId = request.ScenarioName + request.ModelName;

            var lockAcquired = await _lockService.TryLockAsync(lockId);
            if (!lockAcquired)
            {
                throw new InvalidOperationException("Type builder is currently in use. Please try again in a few moments...");
            }

            try
            {
                var response = await _engineRunner.RunAsync(_engine, request);
                return response;
            }
            finally
            {
                _lockService.ReleaseLock(lockId);
            }
        }
    }
}
