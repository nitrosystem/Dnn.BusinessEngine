using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataServices.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Shared.Globals;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Middlewares;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule
{
    public class BuildModuleEngine : EngineBase<BuildModuleRequest, BuildModuleResponse>
    {
        private readonly EnginePipeline<BuildModuleRequest, BuildModuleResponse> _pipeline;

        public BuildModuleEngine(IServiceProvider services)
            : base(services)
        {
            OnError += OnErrorHandle;

            _pipeline = new EnginePipeline<BuildModuleRequest, BuildModuleResponse>()
            .Use<BuildLayoutMiddleware>()
            .Use<MergeResourcesMiddleware>()
            .Use<ResourceAggregatorMiddleware>();
        }

        protected override Task OnInitializeAsync(BuildModuleRequest request)
        {
            var moduleFolder = StringHelper.ToKebabCase(request.ModuleName);
            var outputDirectory = Constants.MapPath(request.BasePath + moduleFolder);
            if (Directory.Exists(outputDirectory))
                Directory.Delete(outputDirectory, true);

            Directory.CreateDirectory(outputDirectory);

            Context.Set("OutputDirectory", outputDirectory);
            Context.Set("OutputRelativePath", request.BasePath + moduleFolder);

            //var logger = Services.GetRequiredService<ILogger<BuildModuleEngine>>();
            //Context.Items["Logger"] = logger;

            //var configService = Services.GetRequiredService<IConfigService>();
            //Context.Items["BuildConfig"] = await configService.GetConfigAsync("BuildModule");

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

            //if (!await _permissionService.HasAccessAsync(Context.UserId, "BuildModule"))
            //    errors.Add("User does not have permission to build module.");

            if (errors.Any())
                return EngineResult<object>.Failure(errors.ToArray());

            return EngineResult<object>.Success(null);
        }

        protected override async Task<EngineResult<BuildModuleResponse>> ExecuteCoreAsync(
            BuildModuleRequest request)
        {
            var lockService = new LockService();

            var lockAcquired = await lockService.TryLockAsync(request.ModuleId.Value);
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
                lockService.ReleaseLock(request.ModuleId.Value);
            }
        }

        private Task OnErrorHandle(Exception ex, string phase)
        {
            throw new NotImplementedException();
        }
    }
}
