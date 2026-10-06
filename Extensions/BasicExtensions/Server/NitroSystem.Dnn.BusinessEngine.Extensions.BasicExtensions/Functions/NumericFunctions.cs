using System;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Attributes;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Functions
{
    public static class NumericFunctions
    {
        [ExpressionFunction("Random")]
        public static int Random(int min, int max)
        {
            return new Random().Next(min, max);
        }
    }
}
