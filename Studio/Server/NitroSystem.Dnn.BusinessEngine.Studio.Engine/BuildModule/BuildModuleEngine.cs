using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Shared.Globals;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Middlewares;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule
{
    public class BuildModuleEngine : EngineBase<BuildModuleRequest, BuildModuleResponse>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ICacheService _cacheService;
        private readonly IModuleService _moduleService;
        private readonly LockService _lockService;

        public BuildModuleEngine(IServiceProvider serviceProvider, ICacheService cacheService, IModuleService moduleService, bool notify)
        {
            _serviceProvider = serviceProvider;
            _cacheService = cacheService;
            _moduleService = moduleService;
            _lockService = serviceProvider.GetRequiredService<LockService>();
        }

        protected override void ConfigurePipeline(EnginePipeline<BuildModuleRequest, BuildModuleResponse> pipeline)
        {
            pipeline
                .Use<BuildLayoutMiddleware>()
                .Use<MergeResourcesMiddleware>()
                .Use<ResourceAggregatorMiddleware>();
        }

        public override BuildModuleResponse CreateEmptyResponse()
        {
            return new BuildModuleResponse();
        }

        protected override Task OnInitializeAsync(BuildModuleRequest request, IEngineContext context)
        {
            if (request.Module.Wrapper == ModuleWrapper.Dashboard)
                request.BasePath = Path.Combine(request.BasePath, StringHelper.ToKebabCase(request.Module.ParentModuleName));

            var moduleFolder = StringHelper.ToKebabCase(request.ModuleName);
            var outputDirectory = Constants.MapPath($"{request.BasePath}/{moduleFolder}");
            var relativeDirectory = $"{request.BasePath}/{moduleFolder}";

            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            context.Set("OutputDirectory", outputDirectory);
            context.Set("OutputRelativePath", relativeDirectory);

            //PushingNotification(request.Module.ScenarioName,
            //    new
            //    {
            //        Type = "ActionCenter",
            //        TaskId = $"{request.ModuleId}-BuildModule",
            //        Message = $"Initialized for build {request.ModuleName} module",
            //        Percent = 5
            //    }
            //);

            return base.OnInitializeAsync(request, context);
        }

        protected override Task ValidateRequestAsync(BuildModuleRequest request, IEngineContext context)
        {
            var errors = new List<string>();

            if (!request.ModuleId.HasValue)
                errors.Add("ModuleId is required.");

            if (string.IsNullOrEmpty(request.ModuleName))
                errors.Add("ModuleName is required.");

            if (errors.Any())
                throw new Exception();

            //PushingNotification(request.Module.ScenarioName,
            //    new
            //    {
            //        Type = "ActionCenter",
            //        TaskId = $"{request.ModuleId}-BuildModule",
            //        Message = $"Validated data for building {request.ModuleName} module",
            //        Percent = 10
            //    }
            //);

            return base.ValidateRequestAsync(request, context);
        }

        protected override async Task BeforeExecuteAsync(BuildModuleRequest request)
        {
            //await _workflow.ExecuteTaskAsync<object>(request.ModuleId.Value.ToString(), request.UserId,
            //    "BuildModuleWorkflow", "BuildModule", "InitialEngine", false, true, true,
            //        (Expression<Func<Task>>)(() => _moduleService.DeleteModuleResourcesAsync(request.ModuleId.Value))
            //    );

            await _moduleService.DeleteModuleResourcesAsync(request.ModuleId.Value);

            // PushingNotification(request.Module.ScenarioName,
            //    new
            //    {
            //        Type = "ActionCenter",
            //        TaskId = $"{request.ModuleId}-BuildModule",
            //        Message = $"Deleting old resources of {request.ModuleName} module",
            //        Percent = 15
            //    }
            //);

            await base.BeforeExecuteAsync(request);
        }

        public async Task<BuildModuleResponse> ExecuteAsync(BuildModuleRequest request)
        {
            var lockAcquired = await _lockService.TryLockAsync(request.ModuleId.Value);
            if (!lockAcquired)
                throw new InvalidOperationException("This module is currently being build. Please try again in a few moments..");

            try
            {
                var runner = _serviceProvider.GetRequiredService<IEngineRunner>();
                var response = await runner.RunAsync(this, request);
                return response;
            }
            finally
            {
                await _lockService.ReleaseLockAsync(request.ModuleId.Value);
            }
        }

        protected override async Task AfterExecuteAsync(BuildModuleRequest request, BuildModuleResponse result)
        {
            if (result.IsSuccess)
            {
                await _moduleService.BulkInsertModuleOutputResourcesAsync(request.Module.SitePageId, result.FinalizedResources);

                //var response = await _workflow.ExecuteTaskAsync<object>(request.ModuleId.Value.ToString(), request.UserId,
                //   "BuildModuleWorkflow", "BuildModule", "ResourceAggregatorMiddleware", false, true, true,
                //  (Expression<Func<Task>>)(() => _moduleService.BulkInsertModuleOutputResourcesAsync(request.Module.SitePageId, result.FinalizedResources))
                //);

                //PushingNotification(request.Module.ScenarioName,
                //    new
                //    {
                //        Type = "ActionCenter",
                //        TaskId = $"{request.ModuleId}-BuildModule",
                //        Message = $"Bulk insert resources in db for {request.ModuleName} module",
                //        Percent = 100,
                //        end = true
                //    }
                //);

                _cacheService.RemoveByPrefix("BE_Modules_");
            }
        }
    }
}
