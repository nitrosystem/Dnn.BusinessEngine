using System;
using System.Linq;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Shared.Helpers
{
    public static class UrlHelper
    {
        public static Dictionary<string, string> ParsePageParameters(string url)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(url))
                return result;

            // تجزیه URL
            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
                return result;

            // 1. پارس پارامترهای query string → ?t=10&u=test
            var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
            foreach (string key in queryParams)
            {
                if (key != null)
                    result[key] = queryParams[key];
            }

            // 2. پارس Segments → /home/t/10/u/test
            var segments = uri.AbsolutePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Skip(1).ToArray();
            for (int i = 0; i < segments.Length - 1; i++)
            {
                string key = segments[i];
                string value = segments[i + 1];

                // بررسی اینکه segment بعدی کلید هست یا مقدار
                if (!result.ContainsKey(key) && !IsNumeric(key) && !IsKeyLikeValue(value))
                {
                    result[key] = value;
                    i++; // چون value رو برداشتیم، یک قدم اضافه بریم
                }
            }

            return result;
        }

        private static bool IsNumeric(string str) => double.TryParse(str, out _);

        private static bool IsKeyLikeValue(string value) =>
            string.IsNullOrWhiteSpace(value) || value.Contains("=");
    }
}
