using System;
using System.Web;
using System.Linq;
using System.Collections.Generic;
using System.Web.Hosting;

namespace NitroSystem.Dnn.BusinessEngine.Shared.Helpers
{
    public static class UrlHelper
    {
        public static Dictionary<string, string> ParsePageParameters(string url)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(url))
                return result;

            // Partition URL
            Uri uri;
            if (Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                // 1. Pars query string params => ?t=10&u=test
                var queryParams = HttpUtility.ParseQueryString(uri.Query);
                foreach (string key in queryParams)
                {
                    if (key != null)
                        result[key] = queryParams[key];
                }

                // 2. Parse Segments => /home/t/10/u/test
                var segments = uri.AbsolutePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                for (int i = 0; i < segments.Length - 1; i++)
                {
                    string key = segments[i];
                    string value = segments[i + 1];

                    // Checking if the next segment is a key or a value.
                    if (!result.ContainsKey(key) && !IsNumeric(key) && !IsKeyLikeValue(value))
                    {
                        result[key] = value;
                        i++; // Since we've taken the value, take one extra step.
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

                // 1. QueryString => ?a=1&b=2
                if (!string.IsNullOrEmpty(query))
                {
                    var qs = HttpUtility.ParseQueryString(query);
                    foreach (string key in qs.AllKeys)
                    {
                        if (!string.IsNullOrWhiteSpace(key))
                            result[key] = qs[key];
                    }
                }

                // 2️. Friendly Segments => /category/tech/page/2
                var segments = path
                    .Trim('/')
                    .Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < segments.Length - 1; i++)
                {
                    string key = segments[i];
                    string value = segments[i + 1];

                    //only logical key/value pattern
                    if (!result.ContainsKey(key) && IsKeyCandidate(key))
                    {
                        result[key] = value;
                        i++; // value used
                    }
                }
            }

            return result;
        }

        private static bool IsNumeric(string str) => double.TryParse(str, out _);

        private static bool IsKeyLikeValue(string value) => string.IsNullOrWhiteSpace(value) || value.Contains("=");

        private static bool IsKeyCandidate(string segment)
        {
            // Key is not be numeric
            if (int.TryParse(segment, out _))
                return false;

            return true;
        }

        public static string ConvertToAbsoluteVirtualPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;

            if (path.StartsWith("~"))
                return VirtualPathUtility.ToAbsolute(path);

            if (path.StartsWith("/"))
                return path;

            string appPath = HostingEnvironment.ApplicationPhysicalPath;
            if (path.StartsWith(appPath, StringComparison.OrdinalIgnoreCase))
            {
                string relative = "~/" + path.Substring(appPath.Length).Replace('\\', '/');
                return VirtualPathUtility.ToAbsolute(relative);
            }

            return VirtualPathUtility.ToAbsolute("~/" + path.TrimStart('/'));
        }
    }
}
