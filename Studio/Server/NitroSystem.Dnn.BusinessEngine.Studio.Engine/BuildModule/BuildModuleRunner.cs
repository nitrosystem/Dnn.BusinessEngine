using System;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.SseNotifier;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule
{
    public class BuildModuleRunner : IBuildModuleRunner
    {
        private readonly IEngineRunner _engineRunner;
        private readonly ISseNotifier _notifier;
        private readonly LockService _lockService;
        private readonly BuildModuleEngine _engine;
        private Guid _moduleId;
        private string _channel;

        public BuildModuleRunner(
            IEngineRunner engineRunner,
            ICacheService cacheService,
            ISseNotifier notifier,
            LockService lockService,
            BuildModuleEngine engine)
        {
            _engineRunner = engineRunner;
            _notifier = notifier;
            _lockService = lockService;
            _engine = engine;
        }

        public async Task<BuildModuleResponse> RunAsync(BuildModuleRequest request)
        {
            _channel = request.Module.ScenarioName;

            var lockAcquired = await _lockService.TryLockAsync(request.Module.Id);
            if (!lockAcquired)
                throw new InvalidOperationException("This module is currently being build. Please try again in a few moments..");

            _moduleId = request.Module.Id;

            try
            {
                await _notifier.Publish(request.Module.ScenarioName,
                    new
                    {
                        channel = request.Module.ScenarioName,
                        type = "ActionCenter",
                        taskId = $"{_moduleId}-BuildModule",
                        icon = "codicon codicon-agent",
                        title = $"Build {request.Module.ModuleName} Module Starting...",
                        subtitle = "The module required rebuild for apply changes",
                        message = $"Starting build {request.Module.ModuleName}",
                        percent = 0,
                    }
                );

                _engine.OnProgress += Engine_OnProgress;

                var response = await _engineRunner.RunAsync(_engine, request);
                if (response.IsSuccess)
                    await Engine_OnProgress("Module build has been successfully!.", 100);
                else
                    throw response.Exception;

                return response;
            }
            finally
            {
                await _lockService.ReleaseLockAsync(request.Module.Id);
            }
        }

        private async Task Engine_OnProgress(string message, double percent, bool isError = false)
        {
            await _notifier.Publish(_channel,
                new
                {
                    channel = _channel,
                    type = "ActionCenter",
                    taskId = $"{_moduleId}-BuildModule",
                    isError = isError,
                    message = message,
                    percent = percent,
                    end = percent == 100
                }
            );
        }
    }
}
