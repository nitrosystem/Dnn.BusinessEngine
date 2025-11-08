using System.Security.Cryptography;
using System.Text;
using System;
using System.Text.RegularExpressions;

namespace NitroSystem.Dnn.BusinessEngine.Shared.Helpers
{
    public static class StringHelper
    {
        /// <summary>
        /// Converts PascalCase or camelCase to kebab-case.
        /// Example: "MyProject2025" => "my-project-2025"
        /// </summary>
        public static string ToKebabCase(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Insert dash before uppercase letters or digit boundaries
            var kebab = Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1-$2");
            kebab = Regex.Replace(kebab, @"([A-Za-z])([0-9])", "$1-$2");
            kebab = Regex.Replace(kebab, @"([0-9])([A-Za-z])", "$1-$2");

            return kebab.ToLowerInvariant();
        }

        public static string BuildKey(string baseKey, params string[] parts)
        {
            // ترکیب کلید پایه با بخش‌های اضافه
            var rawKey = string.IsNullOrEmpty(baseKey)
                ? string.Join("_", parts)
                : baseKey + "_" + string.Join("_", parts);

            // اگر خیلی کوتاه بود، همون رو برگردون
            if (rawKey.Length <= 200)
                return rawKey;

            // در غیر این صورت هش کن
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawKey));
            var hash = Convert.ToBase64String(hashBytes);

            // کلید نهایی: ترکیب baseKey با هش
            return string.IsNullOrEmpty(baseKey) ? hash : $"{baseKey}_{hash}";
        }
    }
}
