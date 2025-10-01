using System;
using NitroSystem.Dnn.BusinessEngine.Shared.Globals;

namespace NitroSystem.Dnn.BusinessEngine.Shared.Helpers
{
    public class CastingHelper
    {
        public static object ConvertStringToObject(string value)
        {
            if (bool.TryParse(value, out bool boolResult))
                return boolResult;
            if (int.TryParse(value, out int intResult))
                return intResult;
            if (long.TryParse(value, out long longResult))
                return longResult;
            if (decimal.TryParse(value, out decimal decimalResult))
                return decimalResult;
            if (double.TryParse(value, out double doubleResult))
                return doubleResult;
            if (DateTime.TryParse(value, out DateTime dateResult))
                return dateResult;

            return value;
        }

        public static string ConvertSqlServerFormatToCSharp(string typeName)
        {
            if (typeName.IndexOf("(") > 0) typeName = typeName.Substring(0, typeName.IndexOf("("));

            var index = Array.IndexOf(Constants.SqlServerTypes, typeName);

            return index > -1
                ? Constants.CSharpTypes[index]
                : "object";
        }

        public static string ConvertCSharpFormatToSqlServer(string typeName)
        {
            var index = Array.IndexOf(Constants.CSharpTypes, typeName);

            return index > -1
                ? Constants.SqlServerTypes[index]
                : null;
        }
    }
}
