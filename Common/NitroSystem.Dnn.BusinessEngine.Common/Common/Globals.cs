using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Utilities
{
    public static class Globals
    {
        public static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public static string GetPersianNumber(string str)
        {
            if (string.IsNullOrEmpty(str)) return str;

            StringBuilder result = new StringBuilder(str);

            for (int i = 0; i < result.Length; i++)
            {
                char ch = str[i];
                if (ch >= '0' && ch <= '9')
                    result[i] = (char)(1776 + char.GetNumericValue(ch));
            }

            return result.ToString();

            //str = str.Replace("0", "۰").Replace("1", "۱").Replace("2", "۲").Replace("3", "۳").Replace("4", "۴").Replace("5", "۵").Replace("6", "۶").Replace("7", "v").Replace("8", "۸").Replace("9", "۹");
        }

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

        public static string GetSafeString(object value)
        {
            return value != DBNull.Value ? (string)value : string.Empty;
        }

        public static int GetSafeInt(object value)
        {
            return value != DBNull.Value ? (int)value : 0;
        }

        public static Guid GetSafeGuid(object value)
        {
            return value != DBNull.Value ? (Guid)value : Guid.Empty;
        }
    }
}
