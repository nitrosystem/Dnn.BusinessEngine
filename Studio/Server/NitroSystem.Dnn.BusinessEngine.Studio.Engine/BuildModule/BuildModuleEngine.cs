using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Core.Workflow;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Shared.Globals;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Middlewares;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule
{
    public class BuildModuleEngine : EngineBase<BuildModuleRequest, BuildModuleResponse>
    {
        private readonly EnginePipeline<BuildModuleRequest, BuildModuleResponse> _pipeline;
        private readonly ICacheService _cacheService;
        private readonly IModuleService _moduleService;
        private readonly WorkflowManager _workflow;
        private readonly LockService _lockService;

        public BuildModuleEngine(IServiceProvider services, ICacheService cacheService, IModuleService moduleService, WorkflowManager workflow, bool notify)
            : base(services, notify)
        {
            _pipeline = new EnginePipeline<BuildModuleRequest, BuildModuleResponse>(this)
            .Use<BuildLayoutMiddleware>()
            .Use<MergeResourcesMiddleware>()
            .Use<ResourceAggregatorMiddleware>();

            _cacheService = cacheService;
            _moduleService = moduleService;
            _workflow = workflow;
            _lockService = services.GetRequiredService<LockService>();
        }

        protected override Task OnInitializeAsync(BuildModuleRequest request)
        {
            if (request.Module.Wrapper == ModuleWrapper.Dashboard)
                request.BasePath = Path.Combine(request.BasePath, StringHelper.ToKebabCase(request.Module.ParentModuleName));

            var moduleFolder = StringHelper.ToKebabCase(request.ModuleName);
            var outputDirectory = Constants.MapPath($"{request.BasePath}/{moduleFolder}");
            var relativeDirectory = $"{request.BasePath}/{moduleFolder}";

            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            Context.Set("OutputDirectory", outputDirectory);
            Context.Set("OutputRelativePath", relativeDirectory);

            PushingNotification(request.Module.ScenarioName,
                new
                {
                    Type = "ActionCenter",
                    TaskId = $"{request.ModuleId}-BuildModule",
                    Message = $"Initialized for build {request.ModuleName} module",
                    Percent = 5
                }
            );

            return base.OnInitializeAsync(request);
        }

        protected override async Task<EngineResult<object>> ValidateAsync(BuildModuleRequest request)
        {
            await Task.Yield();

            var errors = new List<string>();

            if (!request.ModuleId.HasValue)
                errors.Add("ModuleId is required.");

            if (string.IsNullOrEmpty(request.ModuleName))
                errors.Add("ModuleName is required.");

            if (errors.Any())
                return EngineResult<object>.Failure(errors.ToArray());

            PushingNotification(request.Module.ScenarioName,
                new
                {
                    Type = "ActionCenter",
                    TaskId = $"{request.ModuleId}-BuildModule",
                    Message = $"Validated data for building {request.ModuleName} module",
                    Percent = 10
                }
            );

            return EngineResult<object>.Success(null);
        }

        protected override async Task BeforeExecuteAsync(BuildModuleRequest request)
        {
            await _workflow.ExecuteTaskAsync<object>(request.ModuleId.Value.ToString(), request.UserId,
                "BuildModuleWorkflow", "BuildModule", "InitialEngine", false, true, true,
                    (Expression<Func<Task>>)(() => _moduleService.DeleteModuleResourcesAsync(request.ModuleId.Value))
                );

            PushingNotification(request.Module.ScenarioName,
               new
               {
                   Type = "ActionCenter",
                   TaskId = $"{request.ModuleId}-BuildModule",
                   Message = $"Deleting old resources of {request.ModuleName} module",
                   Percent = 15
               }
           );

            await base.BeforeExecuteAsync(request);
        }

        protected override async Task<EngineResult<BuildModuleResponse>> ExecuteCoreAsync(BuildModuleRequest request)
        {
            var lockAcquired = await _lockService.TryLockAsync(request.ModuleId.Value);
            if (!lockAcquired)
            {
                throw new InvalidOperationException("This module is currently being build. Please try again in a few moments..");
            }

            try
            {
                return await _pipeline.ExecuteAsync(request, Context, Services);
            }
            finally
            {
                await _lockService.ReleaseLockAsync(request.ModuleId.Value);
            }
        }

        protected override async Task AfterExecuteAsync(BuildModuleRequest request, EngineResult<BuildModuleResponse> result)
        {
            if (result.Data.IsSuccess)
            {
                var response = await _workflow.ExecuteTaskAsync<object>(request.ModuleId.Value.ToString(), request.UserId,
                   "BuildModuleWorkflow", "BuildModule", "ResourceAggregatorMiddleware", false, true, true,
                  (Expression<Func<Task>>)(() => _moduleService.BulkInsertModuleOutputResourcesAsync(request.Module.SitePageId, result.Data.FinalizedResources))
                );

                PushingNotification(request.Module.ScenarioName,
                    new
                    {
                        Type = "ActionCenter",
                        TaskId = $"{request.ModuleId}-BuildModule",
                        Message = $"Bulk insert resources in db for {request.ModuleName} module",
                        Percent = 100,
                        end = true
                    }
                );

                _cacheService.RemoveByPrefix("BE_Modules_");
            }
        }
    }
}
