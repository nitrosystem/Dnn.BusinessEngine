using System;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.Reflection.TypeGeneration;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Reflection.TypeGeneration.Models;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.TypeBuilder.Middlewares
{
    public class BuildTypeMiddleware : IEngineMiddleware<BuildTypeRequest, BuildTypeResponse>
    {
        private readonly GeneratedModelRegistry _registry;

        public BuildTypeMiddleware(GeneratedModelRegistry registry)
        {
            _registry = registry;
        }

        public async Task<BuildTypeResponse> InvokeAsync(IEngineContext context, BuildTypeRequest request, Func<Task<BuildTypeResponse>> next, Action<string, string, double> progress = null)
        {
            await Task.Yield();

            var outputPath = context.Get<string>("OutputDirectory");
            var relativePath = context.Get<string>("OutputRelativePath");
            var nameSpace = $"NitroSystem.Dnn.BusinessEngine.AppModels.{request.ScenarioName}";
            var typeFullName = $"{nameSpace}.{request.ModelName}";
            var assembly = $@"{outputPath}\{typeFullName}.b";

            var model = new ModelDefinition()
            {
                Namespace = nameSpace,
                Name = request.ModelName,
                ModelVersion = request.Version,
                SchemaVersion = request.Version,
                Properties = request.Properties
            };

            var host = new AssemblyBuilderHost(typeFullName, assembly);
            var factory = new TypeGenerationFactory(host, _registry);
            var type = factory.GetOrBuild(model );

            factory.SaveAssembly();

            var response = new BuildTypeResponse() { RelativePath = relativePath, TypeFullName = typeFullName };
            return response;
        }
    }
}
