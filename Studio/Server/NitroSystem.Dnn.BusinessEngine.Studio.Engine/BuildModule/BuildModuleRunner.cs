using System;
using System.Threading.Tasks;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Controllers;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Core.SseNotifier;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule
{
    public class BuildModuleRunner
    {
        private readonly IEngineRunner _engineRunner;
        private readonly ICacheService _cacheService;
        private readonly IModuleService _moduleService;
        private readonly ISseNotifier _notifier;
        private readonly LockService _lockService;

        public BuildModuleRunner(IEngineRunner engineRunner, ICacheService cacheService, IModuleService moduleService,
            LockService lockService, ISseNotifier notifier)
        {
            _engineRunner = engineRunner;
            _cacheService = cacheService;
            _moduleService = moduleService;
            _lockService = lockService;
            _notifier = notifier;
        }

        public async Task<bool> RunAsync(BuildModuleRequest request)
        {
            var lockAcquired = await _lockService.TryLockAsync(request.Module.Id);
            if (!lockAcquired)
                throw new InvalidOperationException("This module is currently being build. Please try again in a few moments..");

            try
            {
                var engine = new BuildModuleEngine();
                var response = await _engineRunner.RunAsync(engine, request);
                if (response.IsSuccess)
                {
                    await _moduleService.BulkInsertModuleOutputResourcesAsync(request.Module.SitePageId, response.FinalizedResources);

                    _cacheService.RemoveByPrefix("BE_Modules_");
                    HostController.Instance.Update("CrmVersion", (Host.CrmVersion + 1).ToString());

                    await _notifier.Publish(request.Module.ScenarioName,
                        new
                        {
                            Channel = request.Module.ScenarioName,
                            Type = "ActionCenter",
                            TaskId = $"{request.Module.Id}-BuildModule",
                            Message = $"Module build has been successfully!.",
                            Percent = 100,
                            End = true
                        }
                    );
                }

                return response.IsSuccess;
            }
            finally
            {
                await _lockService.ReleaseLockAsync(request.Module.Id);
            }
        }
    }
}
