using System;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Middlewares;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule
{
    public class BuildModuleEngine : EngineBase<BuildModuleRequest, BuildModuleResponse>
    {
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
           Exception ex,
           IEngineContext context,
           BuildModuleResponse response)
        {
            response.IsSuccess = false;

            return Task.CompletedTask;
        }
    }
}
