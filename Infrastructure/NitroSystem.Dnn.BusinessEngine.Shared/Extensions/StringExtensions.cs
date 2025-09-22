using System;

namespace NitroSystem.Dnn.BusinessEngine.Shared.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// اگر رشته نال یا خالی باشد مقدار جایگزین برمی‌گرداند
        /// </summary>
        public static string OrDefault(this string? value, string defaultValue = "")
            => string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();

        /// <summary>
        /// حذف اعداد فارسی و عربی و برگرداندن به اعداد انگلیسی
        /// </summary>
        public static string NormalizeNumbers(this string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return input
                .Replace('۰', '0').Replace('۱', '1').Replace('۲', '2')
                .Replace('۳', '3').Replace('۴', '4').Replace('۵', '5')
                .Replace('۶', '6').Replace('۷', '7').Replace('۸', '8')
                .Replace('۹', '9');
        }
    }
}
