using System.IO;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.TypeGeneration;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.TypeBuilderEngine.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.TypeBuilderEngine.Dto;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.TypeBuilderEngine
{
    public class TypeBuilder : ITypeBuilder
    {
        public string BuildAppModelAsType(AppModelDto appModel, string outputPath)
        {
            if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

            var scenarioName = appModel.ScenarioName.Replace("-", "").Replace(" ", "");
            var nameSpace = $"NitroSystem.Dnn.BusinessEngine.AppModels.{scenarioName}";
            var typeFullName = $"{nameSpace}.{appModel.ModelName}";
            var assembly = $@"{outputPath}\{typeFullName}.dll";

            var model = new ModelDefinition()
            {
                Namespace = nameSpace,
                Name = appModel.ModelName,
                ModelVersion = "01.00.00",
                SchemaVersion = "01.00.00",
                Properties = appModel.Properties
            };

            var host = new AssemblyBuilderHost(typeFullName, assembly);

            var factory = new TypeGenerationFactory(host);
            var type = factory.GetOrBuild(model);

            factory.SaveAssembly();

            return typeFullName;
        }
    }
}
