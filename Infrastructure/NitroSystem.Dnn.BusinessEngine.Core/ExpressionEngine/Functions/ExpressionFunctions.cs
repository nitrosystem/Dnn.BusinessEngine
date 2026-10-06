using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace NitroSystem.Dnn.BusinessEngine.Core.ExpressionEngine.Functions
{
    public static class ExpressionFunctions
    {
        public static readonly ConcurrentDictionary<string, Delegate> BuiltIn = new();

        public static IServiceCollection AddExpressionFunction(this IServiceCollection services, string name, Delegate func)
        {
            BuiltIn.TryAdd(name, func);
            return services;
        }
    }
}
