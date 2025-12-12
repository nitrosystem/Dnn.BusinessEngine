using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace NitroSystem.Dnn.BusinessEngine.Shared.Utils
{
    public static class CacheKeyBuilder
    {
        public static string BuildCacheKey(string baseKey, object parameters = null, string[] columns = null)
        {
            var parts = BuildKeyParts(parameters, columns);

            if (parts.Count == 0)
                return baseKey;

            string raw = $"{baseKey}-{string.Join("|", parts)}";

            if (raw.Length <= 200)
                return raw;

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

            if (parameters != null)
            {
                // ----------------------------
                // ۱) پشتیبانی از ExpandoObject
                // ----------------------------
                if (parameters is IDictionary<string, object> dict)
                {
                    foreach (var kv in dict.OrderBy(x => x.Key))
                    {
                        string value = kv.Value?.ToString() ?? "null";
                        list.Add($"{Encode(kv.Key)}={Encode(value)}");
                    }
                }
                else
                {
                    // ----------------------------
                    // ۲) تمام انواع دیگر با Reflection
                    // ----------------------------
                    var props = parameters.GetType()
                        .GetProperties()
                        .OrderBy(p => p.Name);

                    foreach (var p in props)
                    {
                        var rawValue = p.GetValue(parameters);
                        string value = rawValue?.ToString() ?? "null";
                        list.Add($"{Encode(p.Name)}={Encode(value)}");
                    }
                }
            }

            // ستون‌ها
            if (columns != null && columns.Length > 0)
            {
                foreach (var c in columns.OrderBy(x => x))
                    list.Add($"col_{Encode(c)}");
            }

            return list;
        }


        private static string Encode(string value)
        {
            if (value == null) return "null";

            var bytes = Encoding.UTF8.GetBytes(value);
            var base64 = Convert.ToBase64String(bytes);

            // Base64Url
            return base64
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');
        }
    }
}
