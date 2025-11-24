using System.Linq;
using System.Reflection;
using System.Collections.Concurrent;
using System;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Reflection.Reflection;

namespace NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Export
{
    public abstract class ExportableBase
    {
        protected virtual async Task<T> Export<T>(object instance, Type type, string methodName, params object[] args) where T : class
        {
            var exportableMethods = GetExportableMethods(type);
            if (exportableMethods.TryGetValue(methodName, out var method))
                return await ReflectionService.CallMethodAsync<T>(instance, method, args);

            return default(T);
        }

        private static ConcurrentDictionary<string, MethodInfo> GetExportableMethods(Type type)
        {
            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                              .Where(m => m.GetCustomAttributes(typeof(ExportableAttribute), inherit: false).Length > 0);

            var dict = new ConcurrentDictionary<string, MethodInfo>();

            foreach (var method in methods)
            {
                dict.TryAdd(method.Name, method);
            }

            return dict;
        }
    }
}
