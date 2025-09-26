using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Shared.Reflection
{
    public static class ReflectionUtil
    {
        public static object GetProperty(object src, string propName)
        {
            if (src == null) throw new ArgumentException("Value cannot be null.", "src");
            if (propName == null) throw new ArgumentException("Value cannot be null.", "propName");

            if (propName.Contains("."))//complex type nested
            {
                var temp = propName.Split(new char[] { '.' }, 2);
                return GetProperty(GetProperty(src, temp[0]), temp[1]);
            }
            else
            {
                var prop = src.GetType().GetProperty(propName);
                return prop;
            }
        }

        public static object GetPropertyValue(object obj, string propertyName)
        {
            if (obj == null || string.IsNullOrEmpty(propertyName))
                return null; // جلوگیری از کرش در ورودی نامعتبر

            foreach (var propName in propertyName.Split('.'))
            {
                var prop = obj.GetType().GetProperty(propName);
                if (prop == null)
                    return null; // جلوگیری از NullReferenceException

                obj = prop.GetValue(obj, null);

                if (obj == null)
                    return null; // اگر مقدار null شد، نیازی به ادامه نیست
            }

            return obj;
        }

        public static bool IsStaticClass(Type type)
        {
            return type.IsAbstract && type.IsSealed;
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

        public static async Task<object> CallMethodAsync(object instance, MethodInfo method, params object[] args)
        {
            bool isAsync = method.ReturnType == typeof(Task) ||
                                            (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>));

            object data;
            if (isAsync)
            {
                dynamic task = method.Invoke(instance, args);
                await task;
                data = task.GetAwaiter().GetResult();
            }
            else
            {
                data = method.Invoke(instance, args);
            }

            return data;
        }

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

            MethodInfo method = ReflectionUtil.GetMatchingMethod(type, methodName, args);
            if (method == null)
            {
                throw new ArgumentException($"Method '{methodName}' not found in class '{className}'.");
            }

            var data = method.Invoke(instance, args);

            return (T)data;
        }

    }
}
