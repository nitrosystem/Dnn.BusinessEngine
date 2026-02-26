using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Shared.Helpers
{
    public static class TypeNameHelper
    {
        private static readonly Dictionary<string, string> PrimitiveMap = new()
    {
        { "bool", "System.Boolean" },
        { "byte", "System.Byte" },
        { "sbyte", "System.SByte" },
        { "char", "System.Char" },
        { "short", "System.Int16" },
        { "ushort", "System.UInt16" },
        { "int", "System.Int32" },
        { "uint", "System.UInt32" },
        { "long", "System.Int64" },
        { "ulong", "System.UInt64" },
        { "float", "System.Single" },
        { "double", "System.Double" },
        { "decimal", "System.Decimal" },
        { "string", "System.String" },
        { "object", "System.Object" },
        { "DateTime", "System.DateTime" },
        { "TimeSpan", "System.TimeSpan" },
        { "Guid", "System.Guid" }
        // هرچی لازم داری اضافه کن
    };

        public static Type Resolve(string clrType)
        {
            if (string.IsNullOrWhiteSpace(clrType))
                throw new InvalidOperationException("ClrType is required.");

            // اگر nullable shorthand بود
            if (clrType.EndsWith("?"))
            {
                var baseName = clrType.TrimEnd('?');
                if (PrimitiveMap.TryGetValue(baseName, out var systemName))
                {
                    var nullableName = $"System.Nullable`1[{systemName}]";
                    return Type.GetType(nullableName)
                           ?? throw new InvalidOperationException($"Unable to resolve type '{clrType}'.");
                }
            }

            // حالت غیر nullable
            if (PrimitiveMap.TryGetValue(clrType, out var sysName))
            {
                return Type.GetType(sysName)
                       ?? throw new InvalidOperationException($"Unable to resolve type '{clrType}'.");
            }

            // اگر کاربر AssemblyQualifiedName داده
            return Type.GetType(clrType)
                   ?? throw new InvalidOperationException($"Unable to resolve type '{clrType}'.");
        }
    }

}
