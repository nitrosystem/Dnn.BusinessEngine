using System;

namespace NitroSystem.Dnn.BusinessEngine.Shared.Helpers
{
    public static class UrlHelper
    {
        /// <summary>
        /// ترکیب دو بخش آدرس با رعایت اسلش
        /// </summary>
        public static string Combine(string baseUrl, string relativePath)
        {
            if (string.IsNullOrEmpty(baseUrl)) return relativePath;
            if (string.IsNullOrEmpty(relativePath)) return baseUrl;

            return $"{baseUrl.TrimEnd('/')}/{relativePath.TrimStart('/')}";
        }
    }
}
