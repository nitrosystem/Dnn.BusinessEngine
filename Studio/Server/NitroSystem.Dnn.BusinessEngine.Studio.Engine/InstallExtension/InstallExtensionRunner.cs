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
                //await _notifier.Publish(request.Module.ScenarioName,
                //    new
                //    {
                //        channel = request.Module.ScenarioName,
                //        type = "ActionCenter",
                //        taskId = $"{_moduleId}-InstallExtension",
                //        icon = "codicon codicon-agent",
                //        title = "Build Module Test1...",
                //        subtitle = "The module required rebuild for apply changes",
                //        message = $"Starting build {request.Module.ModuleName}",
                //        percent = 0,
                //    }
                //);

                var engine = new InstallExtensionEngine(_diagnosticStore);
                engine.OnProgress += Engine_OnProgress;

                var response = await _engineRunner.RunAsync(engine, request);
                //if (response.IsSuccess)
                //{
                //    //await Engine_OnProgress(request.Module.ScenarioName, "Module build has been successfully!.", 100);
                //}
                //else
                //    throw response.Exception;

                return response;//.IsSuccess;
            }
            finally
            {
                await _lockService.ReleaseLockAsync(request.Manifest.ExtensionName);
            }
        }

        private async Task Engine_OnProgress(string channel, string message, double percent)
        {
            //await _notifier.Publish(channel,
            //    new
            //    {
            //        channel = channel,
            //        type = "ActionCenter",
            //        taskId = $"{_moduleId}-InstallExtension",
            //        message = message,
            //        percent = percent,
            //        end = percent == 100
            //    }
            //);
        }
    }
}
