using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ModuleBuilder.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts;
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

        private readonly ICacheService _cacheService;
        private readonly IModuleService _moduleService;

        public BuildModuleEngine(IServiceProvider services, ICacheService cacheService, IModuleService moduleService)
            : base(services)
        {
            _pipeline = new EnginePipeline<BuildModuleRequest, BuildModuleResponse>()
            .Use<BuildLayoutMiddleware>()
            .Use<MergeResourcesMiddleware>()
            .Use<ResourceAggregatorMiddleware>();

            _cacheService = cacheService;
            _moduleService = moduleService;

            OnError += OnErrorHandle;
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

            return base.OnInitializeAsync(request);
        }

        protected override async Task BeforeExecuteAsync(BuildModuleRequest request)
        {
            await _moduleService.DeleteModuleResourcesAsync(request.ModuleId.Value);
            await base.BeforeExecuteAsync(request);
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
            try
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
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected override async Task AfterExecuteAsync(BuildModuleRequest request, EngineResult<BuildModuleResponse> result)
        {
            if (result.Data.IsSuccess)
            {
                await _moduleService.BulkInsertModuleOutputResourcesAsync(request.Module.SitePageId, result.Data.FinalizedResources);

                _cacheService.RemoveByPrefix("BE_Modules_");
            }

        }
        private Task OnErrorHandle(Exception ex, string phase)
        {
            throw ex;
        }
    }
}
