using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.BackgroundTaskFramework.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.BackgroundTaskFramework.Models;
using NitroSystem.Dnn.BusinessEngine.Core.Workflow;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Api.BackgroundTask
{
    public class BuildModuleTask : IBackgroundTask
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ICacheService _cacheService;
        private readonly IModuleService _moduleService;
        private readonly WorkflowEventManager _eventManager;
        private readonly Guid _moduleId;
        private readonly int _userId;
        private readonly string _basePath;

        public string TaskId { get; }
        public string Name => "BuildModule";

        public BuildModuleTask(
            IServiceProvider serviceProvider,
            ICacheService cacheService,
            IModuleService moduleService,
            WorkflowEventManager eventManager,
            Guid moduleId,
            int userId,
            string basePath)
        {
            _serviceProvider = serviceProvider;
            _cacheService = cacheService;
            _moduleService = moduleService;
            _eventManager = eventManager;
            _moduleId = moduleId;
            _userId = userId;
            _basePath = basePath;

            TaskId = moduleId.ToString();
        }

        public async Task RunAsync(CancellationToken token, IProgress<ProgressInfo> progress)
        {
            var request = new BuildModuleRequest();
            request.Scope = BuildScope.Module;
            request.UserId = _userId;
            request.BasePath = _basePath;
            request.Module = await _eventManager.ExecuteTaskAsync<ModuleDto>(_moduleId.ToString(), _userId,
                "BuildModuleWorkflow", "BuildModule", "GetDataForBuildModule", false, true, false,
                () => _moduleService.GetDataForModuleBuildingAsync(_moduleId)
             );

            var engine = new BuildModuleEngine(_serviceProvider, _cacheService, _moduleService, _eventManager);
            await engine.ExecuteAsync(request);
        }
    }
}
