using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotNetNuke.Entities.Users;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.TypeBuilder;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Core.BrtPath.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Globals;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.TypeBuilder.Middlewares;
using Microsoft.Extensions.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule
{
    public class TypeBuilderEngine : EngineBase<TypeBuilderRequest, TypeBuilderResponse>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IBrtGateService _brtGate;
        private readonly LockService _lockService;
        private readonly Guid _permitId;

        public TypeBuilderEngine(IServiceProvider serviceProvider, IBrtGateService brtGate, Guid permitId)
        {
            _serviceProvider = serviceProvider;
            _brtGate = brtGate;
            _permitId = permitId;
            _lockService = serviceProvider.GetRequiredService<LockService>();
        }

        protected override void ConfigurePipeline(EnginePipeline<TypeBuilderRequest, TypeBuilderResponse> pipeline)
        {
            pipeline
               .Use<BuildTypeMiddleware>();
        }

        public override TypeBuilderResponse CreateEmptyResponse()
        {
            return new TypeBuilderResponse();
        }

        protected async override Task OnInitializeAsync(TypeBuilderRequest request, IEngineContext context)
        {
            if (!await _brtGate.IsGateOpenAsync(_permitId))
                throw new UnauthorizedAccessException("Operation must run inside BRT gate.");

            var scenarioFolder = StringHelper.ToKebabCase(request.ScenarioName);
            var relativePath = $"{request.BasePath}business-engine/{scenarioFolder}/app-model-types";
            var outputDirectory = Constants.MapPath($@"{request.BasePath}business-engine\{scenarioFolder}\app-model-types");

            if (!Directory.Exists(outputDirectory)) Directory.CreateDirectory(outputDirectory);

            context.Set("PermitId", _permitId);
            context.Set("OutputDirectory", outputDirectory);
            context.Set("OutputRelativePath", relativePath);

            await base.OnInitializeAsync(request, context);
        }

        protected override Task ValidateRequestAsync(TypeBuilderRequest request, IEngineContext context)
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(request.ScenarioName))
                errors.Add("ScenarioName is required.");

            var user = UserController.Instance.GetCurrentUserInfo();
            if (!user.IsInRole("Administrators"))
                errors.Add("User does not have permission to build module.");

            if (errors.Any())
                throw new Exception();

            return base.ValidateRequestAsync(request, context);
        }

        public async Task<TypeBuilderResponse> ExecuteAsync(TypeBuilderRequest request)
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
                var runner = _serviceProvider.GetRequiredService<IEngineRunner>();
                var response = await runner.RunAsync(this, request);
                return response;
            }
            finally
            {
                lockService.ReleaseLock(lockId);
            }
        }
    }
}
