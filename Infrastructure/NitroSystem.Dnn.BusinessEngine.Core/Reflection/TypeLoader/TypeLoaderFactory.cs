using System;
using System.IO;
using System.Reflection;
using System.Security;
using System.Collections.Concurrent;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;

namespace NitroSystem.Dnn.BusinessEngine.Core.Reflection.TypeLoader
{
    public sealed class TypeLoaderFactory : ITypeLoaderFactory
    {
        // Cache: (AssemblyName, TypeFullName) → Type
        private readonly ConcurrentDictionary<string, Type> _typeCache
            = new ConcurrentDictionary<string, Type>(StringComparer.Ordinal);

        // Remain last modified Assembly for invalidate
        private readonly ConcurrentDictionary<string, DateTime> _assemblyLoadTimes
            = new ConcurrentDictionary<string, DateTime>(StringComparer.Ordinal);

        private readonly string _trustedRootNamespace = "NitroSystem.Dnn.BusinessEngine.";

        public Type GetTypeFromAssembly(string relativePath, string typeFullName)
        {
            //ValidatePath(relativePath, basePath, scenarioName);

            var key = $"{relativePath}:{typeFullName}";
            var assemblyPath = GlobalHelper.MapPath(relativePath) + $@"\{typeFullName}.b";

            // If cached
            if (_typeCache.TryGetValue(key, out Type cachedType))
            {
                // If Assembly not modified => return that
                if (IsAssemblyUpToDate(assemblyPath))
                    return cachedType;

                // If modified => invalidate
                InvalidateAssembly(relativePath);
            }

            byte[] asmBytes = File.ReadAllBytes(assemblyPath);
            var assembly = Assembly.Load(asmBytes);

            ValidateAssembly(assembly);

            _assemblyLoadTimes[assemblyPath] = File.GetLastWriteTime(assemblyPath);

            Type type = assembly.GetType(typeFullName, throwOnError: true);
            _typeCache[key] = type; // Save in cache
            return type;
        }

        private void ValidateAssembly(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (!type.FullName.StartsWith(_trustedRootNamespace, StringComparison.Ordinal))
                    throw new SecurityException($"Invalid type found.: {type.FullName}");
            }
        }

        /// <summary>
        ///Checks if the Assembly has been modified on the disk or not.
        /// </summary>
        private bool IsAssemblyUpToDate(string assemblyPath)
        {
            if (_assemblyLoadTimes.TryGetValue(assemblyPath, out DateTime lastLoadTime))
            {
                DateTime currentWriteTime = File.GetLastWriteTime(assemblyPath);
                return currentWriteTime <= lastLoadTime;
            }
            return false;
        }

        /// <summary>
        /// Clear all caches related to an Assembly
        /// </summary>
        public void InvalidateAssembly(string relativePath)
        {
            foreach (var key in _typeCache.Keys)
            {
                if (key.StartsWith(relativePath, StringComparison.Ordinal))
                {
                    _typeCache.TryRemove(key, out _);
                }
            }

            _assemblyLoadTimes.TryRemove(relativePath, out _);
        }

        /// <summary>
        /// Clear all caches
        /// </summary>
        public void ClearAll()
        {
            _typeCache.Clear();
            _assemblyLoadTimes.Clear();
        }
    }
}
