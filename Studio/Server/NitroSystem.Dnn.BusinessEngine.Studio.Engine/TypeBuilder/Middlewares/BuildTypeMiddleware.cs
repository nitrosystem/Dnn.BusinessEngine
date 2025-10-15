using System;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.TypeBuilder;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.TypeGeneration;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Brt.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.TypeBuilder.Middlewares
{
    public class BuildTypeMiddleware : IEngineMiddleware<TypeBuilderRequest, TypeBuilderResponse>
    {
        private readonly IBrtGateService _brtGate;

        public BuildTypeMiddleware(IBrtGateService brtGate)
        {
            _brtGate = brtGate;
        }

        public async Task<EngineResult<TypeBuilderResponse>> InvokeAsync(IEngineContext context, TypeBuilderRequest request, Func<Task<EngineResult<TypeBuilderResponse>>> next)
        {
            var ctx = context as EngineContext;
            var permitId = ctx.Get<Guid>("PermitId");
            var outputPath = ctx.Get<string>("OutputDirectory");
            var relativePath = ctx.Get<string>("OutputRelativePath");
            var nameSpace = $"NitroSystem.Dnn.BusinessEngine.AppModels.{request.ScenarioName}";
            var typeFullName = $"{nameSpace}.{request.ModelName}";
            var assembly = $@"{outputPath}\{typeFullName}.dll";

            var model = new ModelDefinition()
            {
                Namespace = nameSpace,
                Name = request.ModelName,
                ModelVersion = request.Version,
                SchemaVersion = request.Version,
                Properties = request.Properties
            };

            var host = new AssemblyBuilderHost(typeFullName, assembly);
            var factory = new TypeGenerationFactory(host, _brtGate);
            var type = factory.GetOrBuild(model, permitId);

            factory.SaveAssembly();

            var response = new TypeBuilderResponse() { RelativePath = relativePath, TypeFullName = typeFullName };
            return EngineResult<TypeBuilderResponse>.Success(response);
        }
    }
}
