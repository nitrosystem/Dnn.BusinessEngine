using NitroSystem.Dnn.BusinessEngine.Common.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.TypeGeneration
{
    public sealed class PropertyDefinition
    {
        public string Name { get; set; }
        public string ClrType { get; set; } // AssemblyQualifiedName

        public Type ResolveType()
        {
            if (string.IsNullOrWhiteSpace(ClrType)) throw new InvalidOperationException("ClrType is required.");

            Type type = TypeChecker.GetSystemType(ClrType);

            if (type == null) throw new InvalidOperationException($"Unable to resolve type '{ClrType}'.");

            return type;
        }
    }
}
