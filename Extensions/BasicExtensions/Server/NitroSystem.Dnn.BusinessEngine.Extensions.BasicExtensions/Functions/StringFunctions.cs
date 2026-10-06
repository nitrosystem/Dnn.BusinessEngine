using System;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Attributes;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Functions
{
    public static class StringFunctions
    {
        [ExpressionFunction("SubStr")]
        public static string SubStr(object input, int index, int length)
        {
            if (input == null || index < 1 || length <= 0)
                return string.Empty;

            string str = input as string ?? input.ToString();

            int zeroBasedIndex = index - 1;
            if (zeroBasedIndex >= str.Length)
                return string.Empty;

            int actualLength = Math.Min(length, str.Length - zeroBasedIndex);
            return str.Substring(zeroBasedIndex, actualLength);
        }
    }
}
