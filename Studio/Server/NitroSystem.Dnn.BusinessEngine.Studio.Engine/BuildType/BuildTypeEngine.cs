using NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.TypeBuilder;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.TypeBuilder.Middlewares;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule
{
    public class BuildTypeEngine : EngineBase<BuildTypeRequest, BuildTypeResponse>
    {
        public BuildTypeEngine(IDiagnosticStore diagnosticStore) : base(diagnosticStore)
        {
        }

        protected override void ConfigurePipeline(EnginePipeline<BuildTypeRequest, BuildTypeResponse> pipeline)
        {
            pipeline
               .Use<InitializeBuildTypeMiddleware>()
               .Use<BuildTypeMiddleware>();
        }

        protected override BuildTypeResponse CreateEmptyResponse()
        {
            return new BuildTypeResponse();
        }
    }
}
