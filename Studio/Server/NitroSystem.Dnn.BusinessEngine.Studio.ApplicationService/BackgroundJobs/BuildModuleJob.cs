using System;
using System.Threading;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.BackgroundJob;
using NitroSystem.Dnn.BusinessEngine.Core.BackgroundJob.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.BackgroundJobs
{
    public class BuildModuleJob : IJob
    {
        private readonly IModuleService _moduleService;
        private readonly IBuildModuleRunner _buildModuleRunner;

        public BuildModuleJob(IModuleService moduleService, IBuildModuleRunner buildModuleRunner)
        {
            _moduleService = moduleService;
            _buildModuleRunner = buildModuleRunner;
        }

        public async Task RunAsync(JobContext context, CancellationToken token)
        {
            var moduleId = context.Get<Guid>("ModuleId");
            var request = new BuildModuleRequest();
            request.BasePath = context.Get<string>("BasePath");
            request.UserId = context.Get<int>("UserId");
            request.Module = await _moduleService.GetDataForModuleBuildingAsync(moduleId);
            await _buildModuleRunner.RunAsync(request);
        }
    }
}
