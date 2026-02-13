using System;
using System.Linq;
using System.Collections.Generic;
using System.Web;
using System.Security.AccessControl;

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
            if (Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                // 1. پارس پارامترهای query string → ?t=10&u=test
                var queryParams = HttpUtility.ParseQueryString(uri.Query);
                foreach (string key in queryParams)
                {
                    if (key != null)
                        result[key] = queryParams[key];
                }

                // 2. پارس Segments → /home/t/10/u/test
                var segments = uri.AbsolutePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
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
            }
            else
            {
                string path;
                string query = null;

                int qIndex = url.IndexOf('?');
                if (qIndex >= 0)
                {
                    path = url.Substring(0, qIndex);
                    query = url.Substring(qIndex + 1);
                }
                else
                {
                    path = url;
                }

                // 1️⃣ QueryString → ?a=1&b=2
                if (!string.IsNullOrEmpty(query))
                {
                    var qs = HttpUtility.ParseQueryString(query);
                    foreach (string key in qs.AllKeys)
                    {
                        if (!string.IsNullOrWhiteSpace(key))
                            result[key] = qs[key];
                    }
                }

                // 2️⃣ Friendly Segments
                // /category/tech/page/2
                var segments = path
                    .Trim('/')
                    .Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < segments.Length - 1; i++)
                {
                    string key = segments[i];
                    string value = segments[i + 1];

                    // فقط الگوی key/value منطقی
                    if (!result.ContainsKey(key) && IsKeyCandidate(key))
                    {
                        result[key] = value;
                        i++; // value مصرف شد
                    }
                }
            }

            return result;
        }

        private static bool IsNumeric(string str) => double.TryParse(str, out _);

        private static bool IsKeyLikeValue(string value) =>
            string.IsNullOrWhiteSpace(value) || value.Contains("=");

        private static bool IsKeyCandidate(string segment)
        {
            // کلید نباید عددی باشد
            if (int.TryParse(segment, out _))
                return false;

            // کلید نباید خیلی کوتاه باشد (مثلاً slug)
            //if (segment.Length < 2)
            //    return false;

            return true;
        }
    }
}
