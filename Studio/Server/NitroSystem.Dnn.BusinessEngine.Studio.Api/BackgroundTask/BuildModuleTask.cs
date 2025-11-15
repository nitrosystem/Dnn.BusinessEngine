using System;
using System.Threading;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule;
using NitroSystem.Dnn.BusinessEngine.Core.BackgroundTaskFramework.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.BackgroundTaskFramework.Models;
using NitroSystem.Dnn.BusinessEngine.Core.Workflow;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Api.BackgroundTask
{
    public class BuildModuleTask : IBackgroundTask
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ICacheService _cacheService;
        private readonly IModuleService _moduleService;
        private readonly WorkflowEventManager _eventManager;
        private readonly BuildModuleRequest _request;

        public string TaskId { get; }
        public string Name { get; }

        public BuildModuleTask(
            IServiceProvider serviceProvider,
            ICacheService cacheService,
            IModuleService moduleService,
            WorkflowEventManager eventManager,
            BuildModuleRequest request,
            string taskId,
            string name)
        {
            _serviceProvider = serviceProvider;
            _cacheService = cacheService;
            _moduleService = moduleService;
            _eventManager = eventManager;
            _request = request;

            TaskId = taskId;
            Name = name;
        }

        public async Task RunAsync(CancellationToken token, IProgress<ProgressInfo> progress)
        {
            var engine = new BuildModuleEngine(_serviceProvider, _cacheService, _moduleService, _eventManager);
            await engine.ExecuteAsync(_request);
        }
    }
}
