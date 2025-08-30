﻿using Microsoft.Extensions.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.Reflection
{
    public class ServiceLocator : IServiceLocator
    {
        private readonly IServiceProvider _serviceProvider;
        private static readonly ConcurrentDictionary<string, Type> _typeCache = new();

        public ServiceLocator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public T GetInstance<T>(string typeName) where T : class
        {
            var type = ResolveType(typeName);

            if (!typeof(T).IsAssignableFrom(type))
                throw new InvalidCastException($"Type {typeName} is not assignable to {typeof(T).FullName}");

            return _serviceProvider.GetRequiredService(type) as T
                ?? throw new Exception($"Instance not found in DI container for type: {type.FullName}");
        }

        public T CreateInstance<T>(string typeName, params object[] parameters) where T : class
        {
            var type = ResolveType(typeName);

            if (!typeof(T).IsAssignableFrom(type))
                throw new InvalidCastException($"Type {typeName} is not assignable to {typeof(T).FullName}");

            return ActivatorUtilities.CreateInstance(_serviceProvider, type, parameters) as T
                ?? throw new Exception($"Could not create instance of type: {type.FullName}");
        }

        private static Type ResolveType(string typeName)
        {
            return _typeCache.GetOrAdd(typeName, key =>
            {
                var resolvedType = Type.GetType(key) ??
                                   AppDomain.CurrentDomain.GetAssemblies()
                                       .SelectMany(a => a.GetTypes())
                                       .FirstOrDefault(t => t.FullName == key || t.Name == key);

                if (resolvedType == null)
                    throw new TypeLoadException($"Type not found: {key}");

                return resolvedType;
            });
        }
    }
}
