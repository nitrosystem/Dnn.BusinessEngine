using System;
using System.Collections.Concurrent;
using NitroSystem.Dnn.BusinessEngine.Core.Reflection.TypeGeneration.Models;

namespace NitroSystem.Dnn.BusinessEngine.Core.Reflection.TypeGeneration
{
    public sealed class GeneratedModelRegistry
    {
        private readonly ConcurrentDictionary<Type, GeneratedModelDescriptor> _byType
            = new ConcurrentDictionary<Type, GeneratedModelDescriptor>();

        private readonly ConcurrentDictionary<string, Type> _byKey
            = new ConcurrentDictionary<string, Type>();

        public void Register(Type type, GeneratedModelDescriptor descriptor)
        {
            _byType[type] = descriptor;
            _byKey[descriptor.StableKey] = type;
        }

        public bool TryGet(Type type, out GeneratedModelDescriptor descriptor)
            => _byType.TryGetValue(type, out descriptor);

        public bool TryGet(string stableKey, out Type type)
            => _byKey.TryGetValue(stableKey, out type);
    }

}
