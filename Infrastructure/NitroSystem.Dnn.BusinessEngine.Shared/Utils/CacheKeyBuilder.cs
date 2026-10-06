using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
namespace NitroSystem.Dnn.BusinessEngine.Shared.Utils
{
    /// <summary>
    /// Builds deterministic, safe-length cache keys.
    /// If raw key exceeds MaxRawKeyLength, it is hashed (SHA256 + Base64Url).
    /// </summary>
    public static class CacheKeyBuilder
    {

        #region Constants

        private const int MaxRawKeyLength = 200;
        private const string NullToken = "null";

        #endregion

        #region Caches (perf)

        // Cache reflection results per Type to avoid repeated GetProperties() calls.
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _orderedPropsCache = new();

        // ThreadLocal SHA256 to avoid allocating a new instance every call.
        [ThreadStatic]
        private static SHA256 _sha;
        private static SHA256 Sha => _sha ?? (_sha = SHA256.Create());

        #endregion

        #region Public API

        public static string BuildCacheKey(string baseKey, object parameters = null, string[] columns = null)
        {
            if (string.IsNullOrEmpty(baseKey))
                return string.Empty;

            // Fast path: nothing to append
            if (parameters == null && (columns == null || columns.Length == 0))
                return baseKey;

            var sb = new StringBuilder(baseKey.Length + 64);
            sb.Append(baseKey);

            bool first = true;
            AppendParameters(sb, parameters, ref first);
            AppendColumns(sb, columns, ref first);

            if (sb.Length <= MaxRawKeyLength)
                return sb.ToString();

            // Hash long keys to keep them short & unique
            return baseKey + "-" + ComputeHash(sb.ToString());
        }

        #endregion

        #region Builders

        private static void AppendParameters(StringBuilder sb, object parameters, ref bool first)
        {
            if (parameters == null) return;
            if (parameters is IDictionary<string, object> dict)
            {
                // Dictionary path
                foreach (var kv in dict.OrderBy(x => x.Key, StringComparer.Ordinal))
                {
                    AppendSeparator(sb, ref first);
                    sb.Append(Encode(kv.Key));
                    sb.Append('=');
                    sb.Append(Encode(kv.Value?.ToString() ?? NullToken));
                }
                return;
            }

            // POCO path — use cached properties
            var props = _orderedPropsCache.GetOrAdd(parameters.GetType(), t =>
                t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                 .OrderBy(p => p.Name, StringComparer.Ordinal)
                 .ToArray());

            for (int i = 0; i < props.Length; i++)
            {
                var p = props[i];
                var rawValue = p.GetValue(parameters, null);
                AppendSeparator(sb, ref first);
                sb.Append(Encode(p.Name));
                sb.Append('=');
                sb.Append(Encode(rawValue?.ToString() ?? NullToken));
            }
        }

        private static void AppendColumns(StringBuilder sb, string[] columns, ref bool first)
        {
            if (columns == null || columns.Length == 0) return;
            // Avoid LINQ allocations: copy + sort
            var sorted = (string[])columns.Clone();
            Array.Sort(sorted, StringComparer.Ordinal);
            for (int i = 0; i < sorted.Length; i++)
            {
                AppendSeparator(sb, ref first);
                sb.Append("col_");
                sb.Append(Encode(sorted[i]));
            }
        }

        private static void AppendSeparator(StringBuilder sb, ref bool first)
        {
            if (first)
            {
                sb.Append('-');
                first = false;
            }
            else
            {
                sb.Append('|');
            }
        }

        #endregion

        #region Hashing & Encoding

        private static string ComputeHash(string raw)
        {
            var hashBytes = Sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
            return ToBase64Url(hashBytes);
        }

        private static string Encode(string value)
        {
            if (value == null) return NullToken;
            var bytes = Encoding.UTF8.GetBytes(value);
            return ToBase64Url(bytes);
        }

        private static string ToBase64Url(byte[] bytes)
        {
            // Base64Url: '+' -> '-', '/' -> '_', strip '='
            var base64 = Convert.ToBase64String(bytes);
            return base64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }

        #endregion
    }
}
