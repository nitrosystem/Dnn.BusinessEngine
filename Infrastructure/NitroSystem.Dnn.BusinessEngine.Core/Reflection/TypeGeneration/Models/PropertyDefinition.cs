using System;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;

namespace NitroSystem.Dnn.BusinessEngine.Core.Reflection.TypeGeneration.Models
{
    public sealed class PropertyDefinition: IPropertyDefinition
    {
        public string Name { get; set; }
        public string ClrType { get; set; } // AssemblyQualifiedName

        public Type ResolveType()
        {
            if (string.IsNullOrWhiteSpace(ClrType)) throw new InvalidOperationException("ClrType is required.");

            Type type = TypeNameHelper.Resolve(ClrType);
            if (type == null) throw new InvalidOperationException($"Unable to resolve type '{ClrType}'.");

            return type;
        }
    }
}
