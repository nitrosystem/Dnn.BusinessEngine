using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.Reflection
{
    public static class DynamicMethodExecutor
    {
        public static object Execute(
            string expression,
            object[] injectedParams,
            Type targetClass
        )
        {
            // Example: GetFieldTypeBySkin("true", "FieldName", "FieldType", "Template")
            var match = Regex.Match(expression, @"^(?<method>\w+)\((?<args>.*)\)$");
            if (!match.Success)
                throw new ArgumentException("Invalid expression format");

            var methodName = match.Groups["method"].Value;
            var argsString = match.Groups["args"].Value;

            // Split arguments respecting quotes
            var args = Regex.Matches(argsString, @"(""(?:[^""]|"""")*""|\S+)")
                            .Cast<Match>()
                            .Select(m => m.Value.Trim())
                            .ToArray();

            var parsedArgs = args.Select(ParseArg).ToList();

            // Combine injected + parsed args
            var finalArgsList = injectedParams.ToList();
            finalArgsList.AddRange(parsedArgs);

            var allArgs = finalArgsList.ToArray();

            var method = targetClass
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m =>
                {
                    if (m.Name != methodName)
                        return false;

                    var parameters = m.GetParameters();
                    if (parameters.Length != allArgs.Length)
                        return false;

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (allArgs[i] == null)
                            continue; // Null is allowed for reference types

                        if (!parameters[i].ParameterType.IsAssignableFrom(allArgs[i].GetType()))
                            return false;
                    }

                    return true;
                });

            if (method == null)
                throw new MissingMethodException($"Method '{methodName}' with {allArgs.Length} parameters not found in {targetClass.Name}");

            var result = method.Invoke(null, allArgs);

            // If async method
            if (result is Task task)
            {
                task.GetAwaiter().GetResult();

                if (task.GetType().IsGenericType)
                    return task.GetType().GetProperty("Result")?.GetValue(task);

                return null;
            }

            return result;
        }

        private static object ParseArg(string raw)
        {
            if (raw.StartsWith("\"") && raw.EndsWith("\""))
                return raw.Substring(1, raw.Length - 2);
            if (bool.TryParse(raw, out var b))
                return b;
            if (int.TryParse(raw, out var i))
                return i;
            if (Guid.TryParse(raw, out var g))
                return g;

            return raw;
        }

        public static void FirePublicEvent(object onMe, string invokeMe, params object[] eventParams)
        {
            MulticastDelegate eventDelagate =
                  (MulticastDelegate)onMe.GetType().GetField(invokeMe,
                   System.Reflection.BindingFlags.Instance |
                   System.Reflection.BindingFlags.Public).GetValue(onMe);

            Delegate[] delegates = eventDelagate.GetInvocationList();

            foreach (Delegate dlg in delegates)
            {
                dlg.Method.Invoke(dlg.Target, eventParams);
            }
        }
    }
}
