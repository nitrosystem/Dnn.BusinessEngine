using System;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.TypeBuilder
{
    public class BuildTypeRunner
    {
        private readonly IEngineRunner _engineRunner;
        private readonly IDiagnosticStore _diagnosticStore;
        private readonly LockService _lockService;

        public BuildTypeRunner(IEngineRunner engineRunner, IDiagnosticStore diagnosticStore, LockService lockService)
        {
            _engineRunner = engineRunner;
            _diagnosticStore = diagnosticStore;
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
                var engine = new BuildTypeEngine(_diagnosticStore);
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
