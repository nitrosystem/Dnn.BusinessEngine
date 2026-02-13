using System;
using System.Threading.Tasks;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Controllers;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Core.SseNotifier;
using NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule
{
    public class BuildModuleRunner
    {
        private readonly IEngineRunner _engineRunner;
        private readonly ICacheService _cacheService;
        private readonly IDiagnosticStore _diagnosticStore;
        private readonly IModuleService _moduleService;
        private readonly ISseNotifier _notifier;
        private readonly LockService _lockService;
        private Guid _moduleId;

        public BuildModuleRunner(
            IEngineRunner engineRunner,
            ICacheService cacheService,
            ISseNotifier notifier,
            IDiagnosticStore diagnosticStore,
            IModuleService moduleService,
            LockService lockService)
        {
            _engineRunner = engineRunner;
            _cacheService = cacheService;
            _notifier = notifier;
            _diagnosticStore = diagnosticStore;
            _moduleService = moduleService;
            _lockService = lockService;
        }

        public async Task<bool> RunAsync(BuildModuleRequest request)
        {
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
                        title = "Build Module Test1...",
                        subtitle = "The module required rebuild for apply changes",
                        message = $"Starting build {request.Module.ModuleName}",
                        percent = 0,
                    }
                );

                var engine = new BuildModuleEngine(_diagnosticStore);
                engine.OnProgress += Engine_OnProgress;

                var response = await _engineRunner.RunAsync(engine, request);
                if (response.IsSuccess)
                {
                    await _moduleService.BulkInsertModuleOutputResourcesAsync(request.Module.SitePageId, response.FinalizedResources);

                    _cacheService.RemoveByPrefix("BE_Modules_");
                    HostController.Instance.Update("CrmVersion", (Host.CrmVersion + 1).ToString());

                    await Engine_OnProgress(request.Module.ScenarioName, "Module build has been successfully!.", 100);
                }
                else
                    throw response.Exception;

                return response.IsSuccess;
            }
            finally
            {
                await _lockService.ReleaseLockAsync(request.Module.Id);
            }
        }

        private async Task Engine_OnProgress(string channel, string message, double percent)
        {
            await _notifier.Publish(channel,
                new
                {
                    channel = channel,
                    type = "ActionCenter",
                    taskId = $"{_moduleId}-BuildModule",
                    message = message,
                    percent = percent,
                    end = percent == 100
                }
            );
        }
    }
}
