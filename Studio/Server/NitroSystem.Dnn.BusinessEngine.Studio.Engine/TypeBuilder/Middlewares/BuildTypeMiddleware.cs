using System;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.TypeBuilder;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Core.Reflection.TypeGeneration;
using NitroSystem.Dnn.BusinessEngine.Core.BrtPath.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Reflection.TypeGeneration.Models;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.TypeBuilder.Middlewares
{
    public class BuildTypeMiddleware : IEngineMiddleware<TypeBuilderRequest, TypeBuilderResponse>
    {
        private readonly IBrtGateService _brtGate;
        private readonly GeneratedModelRegistry _registry;

        public BuildTypeMiddleware(IBrtGateService brtGate, GeneratedModelRegistry registry)
        {
            _brtGate = brtGate;
            _registry = registry;
        }

        public async Task<TypeBuilderResponse> InvokeAsync(IEngineContext context, TypeBuilderRequest request, Func<Task<TypeBuilderResponse>> next)
        {
            await Task.Yield();

            var permitId = context.Get<Guid>("PermitId");
            var outputPath = context.Get<string>("OutputDirectory");
            var relativePath = context.Get<string>("OutputRelativePath");
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
            var factory = new TypeGenerationFactory(host, _brtGate, _registry);
            var type = factory.GetOrBuild(model, permitId);

            factory.SaveAssembly();

            var response = new TypeBuilderResponse() { RelativePath = relativePath, TypeFullName = typeFullName };
            return response;
        }
    }
}
