using System;
using System.Threading;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.BackgroundJob;
using NitroSystem.Dnn.BusinessEngine.Core.BackgroundJob.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Api.BackgroundJobs
{
    public class BuildModuleJob : IJob
    {
        private readonly IModuleService _moduleService;
        private readonly BuildModuleRunner _buildModuleRunner;

        public BuildModuleJob(IModuleService moduleService, BuildModuleRunner buildModuleRunner)
        {
            _moduleService = moduleService;
            _buildModuleRunner = buildModuleRunner;
        }

        public async Task RunAsync(JobContext context, CancellationToken token)
        {
            var moduleId = context.Get<Guid>("ModuleId");

            var request = new BuildModuleRequest();
            request.Scope = BuildScope.Module;
            request.BasePath = context.Get<string>("BasePath");
            request.UserId = context.Get<int>("UserId");
            request.Module = await _moduleService.GetDataForModuleBuildingAsync(moduleId);

            //request.Module = await _workflow.ExecuteTaskAsync<ModuleDto>(_moduleId.ToString(), _userId,
            //    "BuildModuleWorkflow", "BuildModule", "GetDataForBuildModule", false, true, false,
            //    () => _moduleService.GetDataForModuleBuildingAsync(_moduleId)
            // );

            await _buildModuleRunner.RunAsync(request);
        }
    }
}
