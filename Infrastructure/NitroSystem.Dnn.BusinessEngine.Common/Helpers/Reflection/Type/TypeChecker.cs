using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Common.Reflection
{
    public static class TypeChecker
    {
        public static bool IsValueCompatibleWithType(Type targetType, object value)
        {
            if (value == null)
            {
                // فقط nullable ها می‌تونن مقدار null رو قبول کنن
                return !targetType.IsValueType || Nullable.GetUnderlyingType(targetType) != null;
            }

            try
            {
                // اگر مستقیماً assignable باشه، تاییدش کنیم
                if (targetType.IsInstanceOfType(value))
                    return true;

                // اگر targetType یک Nullable<T> باشه، نوع پایه رو بررسی کن
                var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

                // مخصوص Guid چون Convert.ChangeType پشتیبانی نمی‌کنه
                if (underlyingType == typeof(Guid))
                {
                    if (value is Guid) return true;
                    if (value is string s) return Guid.TryParse(s, out _);
                    return false;
                }

                // مخصوص DateTime چون رشته ممکنه فرمت نادرست داشته باشه
                if (underlyingType == typeof(DateTime))
                {
                    if (value is DateTime) return true;
                    if (value is string s) return DateTime.TryParse(s, out _);
                    return false;
                }

                // برای سایر انواع پایه‌ای
                Convert.ChangeType(value, underlyingType);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static Type GetSystemType(string typeName)
        {
            typeName = typeName ?? string.Empty;

            switch (typeName.ToLower())
            {
                case "object":
                    return typeof(object);
                case "string":
                    return typeof(string);
                case "bool":
                    return typeof(bool);
                case "byte":
                    return typeof(byte);
                case "char":
                    return typeof(char);
                case "decimal":
                    return typeof(decimal);
                case "double":
                    return typeof(double);
                case "short":
                    return typeof(short);
                case "int":
                    return typeof(int);
                case "long":
                    return typeof(long);
                case "sbyte":
                    return typeof(sbyte);
                case "float":
                    return typeof(float);
                case "ushort":
                    return typeof(ushort);
                case "uint":
                    return typeof(uint);
                case "ulong ":
                    return typeof(ulong);
                case "datetime":
                    return typeof(DateTime);
                case "timespan":
                    return typeof(TimeSpan);
                case "guid":
                    return typeof(Guid);
                default:
                    return typeof(object);
                    //return Type.GetType(typeName);
            }
        }

        public static bool CheckDataTypeIsNonObject(Type dataType)
        {
            if (dataType == typeof(string)) return true;
            else if (dataType == typeof(bool)) return true;
            else if (dataType == typeof(byte)) return true;
            else if (dataType == typeof(char)) return true;
            else if (dataType == typeof(decimal)) return true;
            else if (dataType == typeof(double)) return true;
            else if (dataType == typeof(short)) return true;
            else if (dataType == typeof(int)) return true;
            else if (dataType == typeof(long)) return true;
            else if (dataType == typeof(sbyte)) return true;
            else if (dataType == typeof(float)) return true;
            else if (dataType == typeof(ushort)) return true;
            else if (dataType == typeof(uint)) return true;
            else if (dataType == typeof(ulong)) return true;
            else if (dataType == typeof(DateTime)) return true;
            else if (dataType == typeof(TimeSpan)) return true;
            else if (dataType == typeof(Guid)) return true;
            else return false;
        }

        public static object GetSystemTypeDefaultValue(string type)
        {
            switch (type.ToLower())
            {
                case "object":
                    return new System.Object();
                case "string":
                    return string.Empty;
                case "bool":
                    return new System.Boolean();
                case "byte":
                    return new System.Byte();
                case "char":
                    return new System.Char();
                case "decimal":
                    return new System.Decimal();
                case "double":
                    return new System.Double();
                case "short":
                    return new System.Int16();
                case "int":
                    return new System.Int32();
                case "long":
                    return new System.Int64();
                case "sbyte":
                    return new System.SByte();
                case "float":
                    return new System.Double();
                case "datetime":
                    return new System.DateTime();
                case "timespan":
                    return new System.TimeSpan();
                case "guid":
                    return new System.Guid();
                default:
                    return null;
            }
        }
    }
}
