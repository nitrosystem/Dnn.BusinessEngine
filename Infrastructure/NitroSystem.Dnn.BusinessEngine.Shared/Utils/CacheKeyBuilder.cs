namespace NitroSystem.Dnn.BusinessEngine.Shared.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    public static class CacheKeyBuilder
    {
        // تنظیم‌شده: این متد public است تا از بیرون هم قابل فراخوانی باشد
        public static string BuildCacheKey(string baseKey, object parameters = null, string[] columns = null)
        {
            var parts = BuildKeyParts(parameters, columns);

            if (parts.Count == 0)
                return baseKey;

            string raw = $"{baseKey}-{string.Join("|", parts)}";

            // اگر کلید کوتاه بود، همان را استفاده کن
            if (raw.Length <= 200)
                return raw;

            // اگر طولانی بود → SHA256 و Base64Url
            using var sha = SHA256.Create();
            var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
            var hash = Convert.ToBase64String(hashBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');

            return $"{baseKey}-{hash}";
        }

        private static List<string> BuildKeyParts(object parameters, string[] columns = null)
        {
            var list = new List<string>();

            // بخش پارامترهای object (خواص مرتب شده)
            if (parameters != null)
            {
                var props = parameters.GetType()
                    .GetProperties()
                    .OrderBy(p => p.Name);

                foreach (var p in props)
                {
                    var rawValue = p.GetValue(parameters);
                    // اگر خواستی می‌توانی اینجا رفتار دلخواه تبدیل به رشته‌ را تغییر دهی
                    string value = rawValue?.ToString() ?? "null";
                    list.Add($"{Encode(p.Name)}={Encode(value)}");
                }
            }

            // ستون‌ها
            if (columns != null && columns.Length > 0)
            {
                foreach (var c in columns.OrderBy(x => x))
                {
                    list.Add($"col_{Encode(c)}"); // <-- توجه: Add با حرف بزرگ
                }
            }

            return list;
        }

        private static string Encode(string value)
        {
            if (value == null) return "null";

            var bytes = Encoding.UTF8.GetBytes(value);
            var base64 = Convert.ToBase64String(bytes);

            // تبدیل به Base64Url (حذف +, / و =) تا امن برای key شود
            return base64
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');
        }
    }
}
