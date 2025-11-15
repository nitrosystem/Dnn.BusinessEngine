using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotNetNuke.Entities.Users;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.InstallExtension;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.InstallExtension.Models;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Brt.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Globals;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.InstallExtension.Middlewares;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.InstallExtension
{
    public class InstallExtensionEngine : EngineBase<InstallExtensionRequest, InstallExtensionResponse>
    {
        private readonly EnginePipeline<InstallExtensionRequest, InstallExtensionResponse> _pipeline;
        private readonly IBrtGateService _brtGate;
        private readonly IExtensionService _extensionService;
        private readonly Guid _permitId;
        private readonly InstallExtensionContext _ctx;

        public InstallExtensionEngine(
            IServiceProvider services,
            IBrtGateService brtGate,
            IExtensionService extensionService,
            Guid permitId) : base(services)
        {
            _brtGate = brtGate;
            _extensionService = extensionService;
            _permitId = permitId;

            _pipeline = new EnginePipeline<InstallExtensionRequest, InstallExtensionResponse>()
            .Use<SqlDataProviderMiddleware>()
            .Use<ResourcesMiddleware>();

            var cts = new CancellationTokenSource();
            _ctx = new InstallExtensionContext(cts);

            OnError += OnErrorHandle;
        }

        protected async override Task OnInitializeAsync(InstallExtensionRequest request)
        {
            if (!await _brtGate.IsGateOpenAsync(_permitId))
                throw new UnauthorizedAccessException("Operation must run inside BRT gate.");

            var unzipedPath = Constants.MapPath($@"{request.BasePath}business-engine\temp\{Path.GetFileNameWithoutExtension(request.ExtensionZipFile)}");

            Directory.CreateDirectory(unzipedPath);
            ZipProvider.Unzip(request.ExtensionZipFile, unzipedPath);

            var files = Directory.GetFiles(unzipedPath);
            var manifestFile = files.FirstOrDefault(f => Path.GetFileName(f) == "manifest.json");
            var manifestJson = await FileUtil.GetFileContentAsync(manifestFile);

            _ctx.UnzipedPath = unzipedPath;
            _ctx.ExtensionManifest = JsonConvert.DeserializeObject<ExtensionManifest>(manifestJson);

            await base.OnInitializeAsync(request);
        }

        protected override async Task<EngineResult<object>> ValidateAsync(InstallExtensionRequest request)
        {
            var errors = new List<string>();

            var user = UserController.Instance.GetCurrentUserInfo();
            if (!user.IsSuperUser)
                errors.Add("Only superusers can install extensions.");

            var currentVersion = await _extensionService.GetCurrentVersionExtensionsAsync(_ctx.ExtensionManifest.ExtensionName);
            if (!string.IsNullOrEmpty(currentVersion) && new Version(currentVersion) > new Version(_ctx.ExtensionManifest.Version))
                errors.Add("The installed extension should not be larger than the new extension");

            _ctx.CurrentVersion = currentVersion;
            _ctx.ExtensionManifest.IsNewExtension = string.IsNullOrEmpty(currentVersion);

            if (errors.Any())
                return EngineResult<object>.Failure(errors.ToArray());

            return EngineResult<object>.Success(null);
        }

        protected override async Task<EngineResult<InstallExtensionResponse>> ExecuteCoreAsync(
            InstallExtensionRequest request)
        {
            var lockService = new LockService();
            var lockId = _ctx.ExtensionManifest.ExtensionName;

            var lockAcquired = await lockService.TryLockAsync(lockId);
            if (!lockAcquired)
            {
                throw new InvalidOperationException("This type builder is currently being build. Please try again in a few moments..");
            }

            try
            {
                return await _pipeline.ExecuteAsync(request, _ctx, Services);
            }
            catch (Exception ex)
            {
                await OnErrorHandle(ex, "phase");

                return EngineResult<InstallExtensionResponse>.Failure(ex.Message);
            }
            finally
            {
                lockService.ReleaseLock(lockId);
            }
        }

        private async Task OnErrorHandle(Exception ex, string phase)
        {
            await Task.Yield();

            throw ex;
        }
    }
}
