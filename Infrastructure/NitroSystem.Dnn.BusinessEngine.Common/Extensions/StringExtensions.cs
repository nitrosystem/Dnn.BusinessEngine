using NitroSystem.Dnn.BusinessEngine.Shared.Globals;

namespace NitroSystem.Dnn.BusinessEngine.Shared.Extensions
{
    public static class StringExtensions
    {
        public static string ReplaceFrequentTokens(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            foreach (var token in Constants.ModulePopularPaths)
            {
                input = input.Replace(token.Key, token.Value);
            }

            return input;
        }
    }
}
