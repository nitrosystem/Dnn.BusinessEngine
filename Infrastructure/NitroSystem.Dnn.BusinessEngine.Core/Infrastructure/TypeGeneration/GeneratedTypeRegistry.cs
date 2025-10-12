using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.TypeGeneration
{
    public sealed class GeneratedTypeRegistry
    {
        private readonly TypeGenerationFactory _factory;
        private readonly ConcurrentDictionary<string, Type> _types = new ConcurrentDictionary<string, Type>();

        public GeneratedTypeRegistry(TypeGenerationFactory factory)
        {
            _factory = factory;
        }

        public Type Ensure(ModelDefinition def)
        {
            var key = def.ComputeStableKey();
            return _types.GetOrAdd(key, _ => _factory.GetOrBuild(def));
        }
        
        public bool TryGet(string stableKey, out Type type) => _types.TryGetValue(stableKey, out type);
        
        public IReadOnlyDictionary<string, Type> Snapshot() => _types;
    }
}
