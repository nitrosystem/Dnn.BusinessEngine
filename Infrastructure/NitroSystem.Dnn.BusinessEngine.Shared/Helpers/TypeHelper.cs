using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;

namespace NitroSystem.Dnn.BusinessEngine.Shared.Helpers
{
    public static class TypeHelper
    {
        // Pre-boxed default values - cached once, reused forever
        private static readonly Dictionary<string, object> _defaults = new(StringComparer.OrdinalIgnoreCase)
        {
            ["object"] = null,
            ["string"] = string.Empty,
            ["bool"] = default(bool),      // boxed once
            ["byte"] = default(byte),
            ["char"] = default(char),
            ["decimal"] = default(decimal),
            ["double"] = default(double),
            ["short"] = default(short),
            ["int"] = default(int),
            ["long"] = default(long),
            ["sbyte"] = default(sbyte),
            ["float"] = default(float),
            ["ushort"] = default(ushort),
            ["uint"] = default(uint),
            ["ulong"] = default(ulong),
            ["datetime"] = default(DateTime),
            ["timespan"] = default(TimeSpan),
            ["guid"] = default(Guid)
        };
        private static readonly Dictionary<string, string> _primitiveMap = new()
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
        };

        private static readonly HashSet<Type> KnownTypes = new HashSet<Type>
    {
        typeof(bool),
        typeof(byte), typeof(sbyte),
        typeof(short), typeof(ushort),
        typeof(int), typeof(uint),
        typeof(long), typeof(ulong),
        typeof(float), typeof(double), typeof(decimal),
        typeof(char), typeof(string),
        typeof(DateTime), typeof(DateTimeOffset), typeof(TimeSpan),
        typeof(Guid)
    };

        public static object GetSystemTypeDefaultValue(string type)
        {
            if (type == null) return null;

            return _defaults.TryGetValue(type, out var value)
                ? value
                : null;
        }

        public static bool IsValueCompatibleWithType(Type targetType, object value)
        {
            if (value == null)
            {
                //Only nullable types can accept a null value.
                return !targetType.IsValueType || Nullable.GetUnderlyingType(targetType) != null;
            }

            try
            {
                //If it is directly assignable, let's confirm it.
                if (targetType.IsInstanceOfType(value))
                    return true;

                //If targetType is a Nullable<T>, check the base type.
                var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

                // For Guid, since Convert.ChangeType doesn't support it
                if (underlyingType == typeof(Guid))
                {
                    if (value is Guid) return true;
                    if (value is string s) return Guid.TryParse(s, out _);
                    return false;
                }

                // For DateTime, since the string may have an incorrect format
                if (underlyingType == typeof(DateTime))
                {
                    if (value is DateTime) return true;
                    if (value is string s) return DateTime.TryParse(s, out _);
                    return false;
                }

                // For other basic types
                Convert.ChangeType(value, underlyingType);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static Type Resolve(string clrType)
        {
            if (string.IsNullOrWhiteSpace(clrType))
                throw new InvalidOperationException("ClrType is required.");

            // if is nullable => shorthand 
            if (clrType.EndsWith("?"))
            {
                var baseName = clrType.TrimEnd('?');
                if (_primitiveMap.TryGetValue(baseName, out var systemName))
                {
                    var nullableName = $"System.Nullable`1[{systemName}]";
                    return Type.GetType(nullableName)
                           ?? throw new InvalidOperationException($"Unable to resolve type '{clrType}'.");
                }
            }

            // non nullable
            if (_primitiveMap.TryGetValue(clrType, out var sysName))
            {
                return Type.GetType(sysName)
                       ?? throw new InvalidOperationException($"Unable to resolve type '{clrType}'.");
            }

            // if user send AssemblyQualifiedName 
            return Type.GetType(clrType)
                   ?? throw new InvalidOperationException($"Unable to resolve type '{clrType}'.");
        }

        public static Type ResolveType(string typeName)
        {
            return typeName.Trim() switch
            {
                "int?" => typeof(int?),
                "int" => typeof(int),
                "long?" => typeof(long?),
                "long" => typeof(long),
                "double?" => typeof(double?),
                "double" => typeof(double),
                "float?" => typeof(float?),
                "float" => typeof(float),
                "decimal?" => typeof(decimal?),
                "decimal" => typeof(decimal),
                "bool?" => typeof(bool?),
                "bool" => typeof(bool),
                "string" => typeof(string),
                "DateTime?" => typeof(DateTime?),
                "DateTime" => typeof(DateTime),
                "Guid?" => typeof(Guid?),
                "Guid" => typeof(Guid),
                "object" => typeof(object),
                _ => throw new NotSupportedException($"Type '{typeName}' is not supported.")
            };
        }

        public static object ConvertValue(object rawValue, Type targetType)
        {
            // If is null 
            if (rawValue == null || rawValue is string s && string.IsNullOrEmpty(s))
            {
                // nullable & string checking 
                bool isNullable = !targetType.IsValueType ||
                                  Nullable.GetUnderlyingType(targetType) != null;
                if (isNullable) return null;
                throw new InvalidCastException($"Cannot assign null to non-nullable type {targetType}");
            }

            // If the type is same so return rawValue
            if (targetType.IsInstanceOfType(rawValue))
                return rawValue;

            // Get the nullable type (e.g. int? => int)
            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            // Finaly Convert 
            return Convert.ChangeType(rawValue, underlyingType);
        }

        public static Type GetKnownType(object value)
        {
            if (value == null)
                return typeof(object);

            Type type = value.GetType();

            return KnownTypes.Contains(type) ? type : typeof(object);
        }

        public static Type DetectType(string value)
        {
            if (value == null)
                return typeof(string);

            if (value.Length >= 2 && value[0] == '"' && value[value.Length - 1] == '"')
                return typeof(string);

            if (bool.TryParse(value, out _))
                return typeof(bool);

            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
                return typeof(int);

            if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
                return typeof(long);

            if (decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                return typeof(decimal);

            if (Guid.TryParse(value, out _))
                return typeof(Guid);

            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                return typeof(DateTime);

            return typeof(string);
        }

        public static object ConvertValue(string value)
        {
            if (value == null)
                return null;

            if (value.Length >= 2 && value[0] == '"' && value[value.Length - 1] == '"')
                return value.Substring(1, value.Length - 2);

            if (bool.TryParse(value, out bool b))
                return b;

            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int i))
                return i;

            if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long l))
                return l;

            if (decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal dec))
                return dec;

            if (Guid.TryParse(value, out Guid g))
                return g;

            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
                return dt;

            return value;
        }

        public static bool TryGetDictionaryTypes(Type type, out Type keyType, out Type valueType)
        {
            foreach (var itf in type.GetInterfaces().Concat(new[] { type }))
            {
                if (itf.IsGenericType)
                {
                    var def = itf.GetGenericTypeDefinition();
                    if (def == typeof(IDictionary<,>) || def == typeof(IReadOnlyDictionary<,>))
                    {
                        var args = itf.GetGenericArguments();
                        keyType = args[0];
                        valueType = args[1];
                        return true;
                    }
                }
            }

            keyType = null;
            valueType = null;
            return false;
        }
    }
}

