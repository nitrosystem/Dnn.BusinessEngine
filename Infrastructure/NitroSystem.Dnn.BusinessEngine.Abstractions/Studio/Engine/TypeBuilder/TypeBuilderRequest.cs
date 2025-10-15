using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.TypeBuilder
{
    public class TypeBuilderRequest
    {
        public string ScenarioName { get; set; }
        public string BasePath { get; set; }
        public string ModelName { get; set; }
        public string Version { get; set; }
        public List<IPropertyDefinition> Properties { get; set; }
    }
}
