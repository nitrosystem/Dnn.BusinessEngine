using System;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.InstallExtension;
using NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.InstallExtension.Middlewares;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.InstallExtension
{
    public class InstallExtensionEngine : EngineBase<InstallExtensionRequest, InstallExtensionResponse>
    {
        public InstallExtensionEngine(IDiagnosticStore diagnosticStore) 
            : base(diagnosticStore)
        {
        }

        protected override void ConfigurePipeline(EnginePipeline<InstallExtensionRequest, InstallExtensionResponse> pipeline)
        {
            pipeline
                .Use<ValidateMiddleware>()
                .Use<SqlDataProviderMiddleware>()
                .Use<ResourcesMiddleware>();
        }

        protected override InstallExtensionResponse CreateEmptyResponse()
        {
            return new InstallExtensionResponse();
        }

        protected override Task OnErrorAsync(
           IEngineContext context,
           InstallExtensionRequest request,
           InstallExtensionResponse response,
           Exception ex)
        {
            return Task.CompletedTask;
        }
    }

}
