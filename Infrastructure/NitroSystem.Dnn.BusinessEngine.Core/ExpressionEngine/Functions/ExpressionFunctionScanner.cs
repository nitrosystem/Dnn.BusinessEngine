using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Attributes;

namespace NitroSystem.Dnn.BusinessEngine.Core.ExpressionEngine.Functions
{
    public static class ExpressionFunctionScanner
    {
        public static void ScanAndRegister(IEnumerable<Assembly> assemblies)
        {
            var methods = assemblies
                .SelectMany(a => a.GetTypes())
                .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public))
                .Select(m => (Method: m, Attr: m.GetCustomAttribute<ExpressionFunctionAttribute>()))
                .Where(x => x.Attr != null);

            foreach (var (method, attr) in methods)
            {
                var paramTypes = method.GetParameters().Select(p => p.ParameterType)
                    .Append(method.ReturnType).ToArray();
                var delegateType = Expression.GetDelegateType(paramTypes);
                var del = method.CreateDelegate(delegateType);

                if (!ExpressionFunctions.BuiltIn.TryAdd(attr.Name, del))
                    throw new InvalidOperationException(
                        $"Duplicate expression function name '{attr.Name}' from {method.DeclaringType!.FullName}.");
            }
        }
    }
}
