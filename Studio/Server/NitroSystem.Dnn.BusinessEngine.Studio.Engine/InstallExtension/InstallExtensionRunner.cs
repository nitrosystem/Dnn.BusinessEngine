using System;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.InstallExtension;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.InstallExtension.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.SseNotifier;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.InstallExtension
{
    public class InstallExtensionRunner: IInstallExtensionRunner
    {
        private readonly IEngineRunner _engineRunner;
        private readonly ISseNotifier _notifier;
        private readonly LockService _lockService;
        private readonly InstallExtensionEngine _engine;
        private string _channel;
        private string _extensionName;

        public InstallExtensionRunner(
            IEngineRunner engineRunner,
            ISseNotifier notifier,
            LockService lockService,
            InstallExtensionEngine engine)
        {
            _engineRunner = engineRunner;
            _notifier = notifier;
            _lockService = lockService;
            _engine = engine;
        }

        public async Task<InstallExtensionResponse> RunAsync(InstallExtensionRequest request)
        {
            var lockAcquired = await _lockService.TryLockAsync(request.Manifest.ExtensionName);
            if (!lockAcquired)
                throw new InvalidOperationException("Install extension is currently in use. Please try again in a few moments...");

            try
            {
                _channel = request.Channel;
                _extensionName = request.Manifest.ExtensionName;

                await Engine_OnProgress($"Starting install {request.Manifest.ExtensionName} extension...", 0);
                _engine.OnProgress += Engine_OnProgress;

                var response = await _engineRunner.RunAsync(_engine, request);
                if (response.IsSuccess)
                {
                    await Engine_OnProgress($"{request.Manifest.ExtensionName} extension has been installing successfully!.", 100);
                }
                else
                {
                    throw response.Exception;
                }

                return response;
            }
            finally
            {
                await _lockService.ReleaseLockAsync(request.Manifest.ExtensionName);
            }
        }

        private async Task Engine_OnProgress(string message, double percent, bool isError = false)
        {
            await _notifier.Publish(_channel,
                new
                {
                    channel = _channel,
                    type = "InstallExtension",
                    taskId = $"{_extensionName}-installing",
                    isError = isError,
                    message = message,
                    percent = percent
                }
            );

            await Task.Delay(500);
        }
    }
}
