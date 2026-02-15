using System;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Core.SseNotifier;
using NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.InstallExtension
{
    public class InstallExtensionRunner
    {
        private readonly IEngineRunner _engineRunner;
        private readonly IDiagnosticStore _diagnosticStore;
        private readonly ISseNotifier _notifier;
        private readonly LockService _lockService;
        private string _channel;
        private string _extensionName;

        public InstallExtensionRunner(
            IEngineRunner engineRunner,
            ISseNotifier notifier,
            LockService lockService,
            IDiagnosticStore diagnosticStore)
        {
            _engineRunner = engineRunner;
            _notifier = notifier;
            _lockService = lockService;
            _diagnosticStore = diagnosticStore;
        }

        public async Task<InstallExtensionResponse> RunAsync(InstallExtensionRequest request)
        {
            var lockAcquired = await _lockService.TryLockAsync(request.Manifest.ExtensionName);
            if (!lockAcquired)
                throw new InvalidOperationException("This extension is currently being installing. Please try again in a few moments..");

            try
            {
                _channel = request.Channel;
                _extensionName = request.Manifest.ExtensionName;

                await Engine_OnProgress($"Starting install {request.Manifest.ExtensionName} extension...", 0);

                var engine = new InstallExtensionEngine(_diagnosticStore);
                engine.OnProgress += Engine_OnProgress;

                var response = await _engineRunner.RunAsync(engine, request);
                if (response.IsSuccess)
                {
                    await Engine_OnProgress($"{request.Manifest.ExtensionName} extension has been installing successfully!.", 100);
                }
                else
                    throw response.Exception;

                return response;
            }
            finally
            {
                await _lockService.ReleaseLockAsync(request.Manifest.ExtensionName);
            }
        }

        private async Task Engine_OnProgress(string message, double percent)
        {
            await _notifier.Publish(_channel,
                new
                {
                    channel = _channel,
                    type = "InstallExtension",
                    taskId = $"{_extensionName}-installing",
                    message = message,
                    percent = percent
                }
            );

            await Task.Delay(500);
        }
    }
}
