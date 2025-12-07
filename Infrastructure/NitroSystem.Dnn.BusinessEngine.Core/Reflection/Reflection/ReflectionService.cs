using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.Reflection.Reflection
{
    public class ReflectionService
    {
        public static T CallMethod<T>(string className, string methodName, params object[] args)
        {
            object instance = null;

            Type type = Type.GetType(className);
            if (type == null)
            {
                throw new ArgumentException($"Class '{className}' not found.");
            }

            if (!IsStaticClass(type))
            {
                instance = Activator.CreateInstance(type);
                if (instance == null)
                {
                    throw new ArgumentException($"Class '{className}' not found.");
                }
            }

            MethodInfo method = GetMatchingMethod(type, methodName, args);
            if (method == null)
            {
                throw new ArgumentException($"Method '{methodName}' not found in class '{className}'.");
            }

            var data = method.Invoke(instance, args);

            return (T)data;
        }

        public static async Task<T> CallMethodAsync<T>(Type type, string methodName, params object[] args) where T : class
        {
            var method = GetMatchingMethod(type, methodName, args);
            var data = await CallMethodAsync<T>(type, method, args);
            return data;
        }

        public static async Task<T> CallMethodAsync<T>(object instance, MethodInfo method, params object[] args) where T : class
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            // اجرای متد
            var result = method.Invoke(instance, args);

            // اگر متد Async است
            if (result is Task task)
            {
                await task.ConfigureAwait(false);

                // اگر متد از نوع Task<T> است، مقدار را برمی‌گردانیم
                if (method.ReturnType.IsGenericType)
                {
                    var resultProperty = method.ReturnType.GetProperty("Result");
                    return (T)resultProperty?.GetValue(task);
                }

                return default;
            }

            // متد Sync
            return result as T;
        }

        public static MethodInfo GetMatchingMethod(Type type, string methodName, params object[] args)
        {
            MethodInfo[] methods = type.GetMethods()
                                       .Where(m => m.Name == methodName)
                                       .ToArray();

            foreach (var method in methods)
            {
                var methodParams = method.GetParameters();

                if (methodParams.Length != args.Length)
                    continue;

                bool match = true;
                for (int i = 0; i < methodParams.Length; i++)
                {
                    if (args.GetType().IsAssignableFrom(methodParams[i].ParameterType))
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                    return method;
            }

            return null;
        }

        private static bool IsStaticClass(Type type)
        {
            return type.IsAbstract && type.IsSealed;
        }
    }
}
