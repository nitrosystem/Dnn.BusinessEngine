using System;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Middlewares;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule
{
    public class BuildModuleEngine : EngineBase<BuildModuleRequest, BuildModuleResponse>
    {
        public BuildModuleEngine(IDiagnosticStore diagnosticStore) : base(diagnosticStore)
        {
        }

        protected override void ConfigurePipeline(EnginePipeline<BuildModuleRequest, BuildModuleResponse> pipeline)
        {
            pipeline
                .Use<InitializeBuildModuleMiddleware>()
                .Use<DeleteOldResourcesMiddleware>()
                .Use<BuildLayoutMiddleware>()
                .Use<MergeResourcesMiddleware>()
                .Use<ResourceAggregatorMiddleware>();
        }

        protected override BuildModuleResponse CreateEmptyResponse()
        {
            return new BuildModuleResponse();
        }

        protected override Task OnErrorAsync(
           IEngineContext context,
           BuildModuleRequest request,
           BuildModuleResponse response,
           Exception ex)
        {
            response.IsSuccess = false;
            response.Exception = ex;

            return Task.CompletedTask;
        }
    }
}
