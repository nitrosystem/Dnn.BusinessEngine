using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotNetNuke.Entities.Users;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.TypeBuilder;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Brt.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Globals;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.TypeBuilder.Middlewares;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule
{
    public class TypeBuilderEngine : EngineBase<TypeBuilderRequest, TypeBuilderResponse>
    {
        private readonly EnginePipeline<TypeBuilderRequest, TypeBuilderResponse> _pipeline;
        private readonly IBrtGateService _brtGate;
        private readonly Guid _permitId;

        public TypeBuilderEngine(IServiceProvider services, IBrtGateService brtGate, Guid permitId)
            : base(services)
        {
            _brtGate = brtGate;
            _permitId = permitId;

            _pipeline = new EnginePipeline<TypeBuilderRequest, TypeBuilderResponse>()
            .Use<BuildTypeMiddleware>();

            OnError += OnErrorHandle;
        }

        protected async override Task OnInitializeAsync(TypeBuilderRequest request)
        {
            if (!await _brtGate.IsGateOpenAsync(_permitId))
                throw new UnauthorizedAccessException("Operation must run inside BRT gate.");

            var scenarioFolder = StringHelper.ToKebabCase(request.ScenarioName);
            var relativePath = $"{request.BasePath}business-engine/{scenarioFolder}/app-model-types";
            var outputDirectory = Constants.MapPath($@"{request.BasePath}business-engine\{scenarioFolder}\app-model-types");

            if (!Directory.Exists(outputDirectory)) Directory.CreateDirectory(outputDirectory);

            Context.Set("PermitId", _permitId);
            Context.Set("OutputDirectory", outputDirectory);
            Context.Set("OutputRelativePath", relativePath);

            await base.OnInitializeAsync(request);
        }

        protected override async Task<EngineResult<object>> ValidateAsync(TypeBuilderRequest request)
        {
            await Task.Yield();

            var errors = new List<string>();

            if (string.IsNullOrEmpty(request.ScenarioName))
                errors.Add("ScenarioName is required.");

            var user = UserController.Instance.GetCurrentUserInfo();
            if (!user.IsInRole("Administrators"))
                errors.Add("User does not have permission to build module.");

            if (errors.Any())
                return EngineResult<object>.Failure(errors.ToArray());

            return EngineResult<object>.Success(null);
        }

        protected override async Task<EngineResult<TypeBuilderResponse>> ExecuteCoreAsync(
            TypeBuilderRequest request)
        {
            var lockService = new LockService();
            var lockId = request.ScenarioName + request.ModelName;

            var lockAcquired = await lockService.TryLockAsync(lockId);
            if (!lockAcquired)
            {
                throw new InvalidOperationException("This type builder is currently being build. Please try again in a few moments..");
            }

            try
            {
                return await _pipeline.ExecuteAsync(request, Context, Services);
            }
            finally
            {
                lockService.ReleaseLock(lockId);
            }
        }

        private Task OnErrorHandle(Exception ex, string phase)
        {
            throw new NotImplementedException();
        }
    }
}
