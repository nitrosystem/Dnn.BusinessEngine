using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Core.Reflection.TypeGeneration.Models
{
    public sealed class GeneratedModelDescriptor
    {
        public string Name { get; }
        public string Version { get; }
        public string StableKey { get; }
        public IReadOnlyList<IPropertyDefinition> Properties { get; }

        public GeneratedModelDescriptor(
            string name,
            string version,
            string stableKey,
            IReadOnlyList<IPropertyDefinition> properties)
        {
            Name = name;
            Version = version;
            StableKey = stableKey;
            Properties = properties;
        }
    }
}
