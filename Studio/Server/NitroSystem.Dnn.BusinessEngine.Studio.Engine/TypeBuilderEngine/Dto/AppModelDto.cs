using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.TypeGeneration;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.TypeBuilderEngine.Dto
{
   public class AppModelDto
    {
        public string ModelName { get; set; }
        public string ScenarioName { get; set; }
        public List<PropertyDefinition> Properties { get; set; }
    }
}
