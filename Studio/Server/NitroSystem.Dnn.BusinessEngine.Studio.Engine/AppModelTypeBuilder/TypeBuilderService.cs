using NitroSystem.Dnn.BusinessEngine.Core.TypeGeneration;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.Dto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.AppModelTypeBuilder
{
    public class TypeBuilderService : ITypeBuilderService
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
