using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Abstractions.BuildModuleEngine;
using NitroSystem.Dnn.BusinessEngine.Abstractions.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Shared.Globals;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Studio.DataServices.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Middlewares;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule
{
    public class BuildModuleEngine : EngineBase<BuildModuleRequest, BuildModuleResponse>
    {
        private readonly EnginePipeline<BuildModuleRequest, BuildModuleResponse> _pipeline;

        private readonly IModuleService _moduleService;

        public BuildModuleEngine(IServiceProvider services, IModuleService _moduleService, EngineContext context)
            : base(context)
        {
            OnError += OnErrorHandle;

            _pipeline = new EnginePipeline<BuildModuleRequest, BuildModuleResponse>()
            .Use<BuildLayoutMiddleware>()
            .Use<MergeResourcesMiddleware>();
        }

        protected override Task OnInitializeAsync(BuildModuleRequest request, EngineContext context)
        {
            var outputDirectory = Constants.MapPath(request.BasePath + request.ModuleName);
            if (Directory.Exists(outputDirectory))
                FileUtil.DeleteDirectory(outputDirectory, true);

            //var logger = Services.GetRequiredService<ILogger<BuildModuleEngine>>();
            //context.Items["Logger"] = logger;

            //var configService = Services.GetRequiredService<IConfigService>();
            //context.Items["BuildConfig"] = await configService.GetConfigAsync("BuildModule");

            return base.OnInitializeAsync(request, context);
        }

        protected override async Task<EngineResult<object>> ValidateAsync(BuildModuleRequest request, EngineContext context)
        {
            await Task.Yield();

            var errors = new List<string>();

            if (!request.ModuleId.HasValue)
                errors.Add("ModuleId is required.");

            if (string.IsNullOrEmpty(request.ModuleName))
                errors.Add("ModuleName is required.");

            //if (!await _permissionService.HasAccessAsync(context.UserId, "BuildModule"))
            //    errors.Add("User does not have permission to build module.");

            if (errors.Any())
                return EngineResult<object>.Failure(errors.ToArray());

            return EngineResult<object>.Success(null);
        }

        protected override async Task BeforeExecuteAsync(BuildModuleRequest request, EngineContext context)
        {
            await _moduleService.DeleteModuleResourcesAsync(request.ModuleId.Value);
        }

        protected override async Task<EngineResult<BuildModuleResponse>> ExecuteCoreAsync(
            BuildModuleRequest request, EngineContext context)
        {
            var lockService = new LockService();

            var lockAcquired = await lockService.TryLockAsync(request.ModuleId.Value);
            if (!lockAcquired)
            {
                throw new InvalidOperationException("This module is currently being build. Please try again in a few moments..");
            }

            try
            {
                return await _pipeline.ExecuteAsync(request, context, Services);
            }
            finally
            {
                lockService.ReleaseLock(request.ModuleId.Value);
            }
        }

        private Task OnErrorHandle(Exception ex, string phase)
        {
            throw new NotImplementedException();
        }
    }
}
