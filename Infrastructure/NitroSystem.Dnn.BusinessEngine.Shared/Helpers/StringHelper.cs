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
    }
}
