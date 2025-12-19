using System;
using System.Reflection;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Core.DSLEngine
{
    public sealed class MemberAccessResolver
    {
        private readonly Dictionary<string, PropertyInfo> _cache
            = new Dictionary<string, PropertyInfo>();

        public object GetValue(object target, string member)
        {
            var key = target.GetType().FullName + "." + member;

            PropertyInfo prop;
            if (!_cache.TryGetValue(key, out prop))
            {
                prop = target.GetType().GetProperty(member,
                    BindingFlags.Instance | BindingFlags.Public);

                if (prop == null)
                    throw new InvalidOperationException("Invalid member: " + member);

                _cache[key] = prop;
            }

            return prop.GetValue(target, null);
        }

        public void SetValue(object target, string member, object value)
        {
            var key = target.GetType().FullName + "." + member;

            PropertyInfo prop;
            if (!_cache.TryGetValue(key, out prop))
            {
                prop = target.GetType().GetProperty(
                    member,
                    BindingFlags.Instance | BindingFlags.Public);

                if (prop == null || !prop.CanWrite)
                    throw new InvalidOperationException(
                        "Invalid assignment target: " + member);

                _cache[key] = prop;
            }

            object convertedValue = ConvertValue(value, prop.PropertyType);

            prop.SetValue(target, convertedValue, null);
        }

        private static object ConvertValue(object value, Type targetType)
        {
            // null assignment
            if (value == null)
            {
                if (IsNullable(targetType))
                    return null;

                throw new InvalidOperationException(
                    $"Cannot assign null to non-nullable type '{targetType}'");
            }

            var valueType = value.GetType();

            // exact match
            if (targetType.IsAssignableFrom(valueType))
                return value;

            // Nullable<T>
            var underlying = Nullable.GetUnderlyingType(targetType);
            if (underlying != null)
            {
                var converted = ConvertValue(value, underlying);
                return Activator.CreateInstance(targetType, converted);
            }

            // Enum
            if (targetType.IsEnum)
            {
                if (value is string)
                    return Enum.Parse(targetType, value.ToString(), true);

                return Enum.ToObject(targetType, value);
            }

            // numeric & primitive conversions
            return Convert.ChangeType(value, targetType);
        }

        private static bool IsNullable(Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }
    }
}
