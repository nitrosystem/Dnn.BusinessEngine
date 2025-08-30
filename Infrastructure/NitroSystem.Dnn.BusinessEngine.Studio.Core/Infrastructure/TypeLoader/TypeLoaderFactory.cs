﻿using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Entities.Portals;
using global::NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Reflection.TypeLoader;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using System;
using System.Collections.Concurrent;
using System.Configuration.Assemblies;
using System.IO;
using System.Reflection;
using System.Security;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.Core.Reflection.TypeLoader
{
    public sealed class TypeLoaderFactory : ITypeLoaderFactory
    {
        // Cache: (AssemblyName, TypeFullName) → Type
        private readonly ConcurrentDictionary<string, Type> _typeCache
            = new ConcurrentDictionary<string, Type>(StringComparer.Ordinal);

        // نگهداری آخرین زمان تغییر Assembly برای invalidate
        private readonly ConcurrentDictionary<string, DateTime> _assemblyLoadTimes
            = new ConcurrentDictionary<string, DateTime>(StringComparer.Ordinal);

        private readonly string _trustedRootNamespace = "NitroSystem.Dnn.BusinessEngine.";

        public Type GetTypeFromAssembly(string relativePath, string typeFullName, string scenarioName, PortalSettings portalSettings)
        {
            ValidatePath(relativePath, portalSettings.HomeSystemDirectory, scenarioName);

            var key = $"{relativePath}:{typeFullName}";
            var assemblyPath = HttpContext.Current.Server.MapPath($"{relativePath + typeFullName}.dll");

            // اگر قبلاً کش شده
            if (_typeCache.TryGetValue(key, out Type cachedType))
            {
                // اگر Assembly تغییر نکرده → همون رو بده
                if (IsAssemblyUpToDate(assemblyPath))
                    return cachedType;

                // اگر تغییر کرده → invalidate کن
                InvalidateAssembly(relativePath);
            }

            //AppDomainSetup setup = new AppDomainSetup
            //{
            //    ApplicationBase = Path.GetDirectoryName(assemblyPath),
            //    PrivateBinPath = Path.GetDirectoryName(relativePath)
            //};

            //AppDomain pluginDomain = AppDomain.CreateDomain("BusinessEngine", null, setup);

            //var loader = (PluginLoader)pluginDomain.CreateInstanceAndUnwrap(
            //    typeof(PluginLoader).Assembly.FullName,
            //    typeof(PluginLoader).FullName);

            //var assembly = loader.Load(assemblyPath, typeFullName);

            var assembly = Assembly.LoadFrom(assemblyPath);
            ValidateAssembly(assembly);

            _assemblyLoadTimes[assemblyPath] = File.GetLastWriteTime(assemblyPath);

            // گرفتن نوع
            Type type = assembly.GetType(typeFullName, throwOnError: true);

            // ذخیره در کش
            _typeCache[key] = type;

            return type;
        }

        private void ValidatePath(string relativePath, string basePath, string scenarioName)
        {
            var trustedPathPrefix = $"{basePath}/business-engine/{scenarioName}/";

            if (!relativePath.StartsWith(trustedPathPrefix, StringComparison.OrdinalIgnoreCase))
                throw new SecurityException("مسیر پلاگین مجاز نیست.");
        }

        private void ValidateAssembly(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (!type.FullName.StartsWith(_trustedRootNamespace, StringComparison.Ordinal))
                    throw new SecurityException($"تایپ غیرمجاز یافت شد: {type.FullName}");
            }
        }

        /// <summary>
        /// بررسی می‌کنه که Assembly روی دیسک تغییر کرده یا نه
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
        /// پاک کردن همه‌ی کش‌های مربوط به یک Assembly
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
        /// پاک کردن کل کش
        /// </summary>
        public void ClearAll()
        {
            _typeCache.Clear();
            _assemblyLoadTimes.Clear();
        }
    }
}
