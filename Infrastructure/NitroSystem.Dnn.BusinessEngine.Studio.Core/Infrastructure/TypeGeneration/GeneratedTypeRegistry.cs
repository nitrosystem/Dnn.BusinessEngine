using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.TypeGeneration
{
    public sealed class GeneratedTypeRegistry
    {
        private readonly NitroSystem.Dnn.BusinessEngine.Core.TypeGeneration.TypeGenerationFactory _factory;
        private readonly ConcurrentDictionary<string, Type> _types = new ConcurrentDictionary<string, Type>();

        public GeneratedTypeRegistry(NitroSystem.Dnn.BusinessEngine.Core.TypeGeneration.TypeGenerationFactory factory)
        {
            _factory = factory;
        }

        public Type Ensure(NitroSystem.Dnn.BusinessEngine.Core.TypeGeneration.ModelDefinition def)
        {
            var key = def.ComputeStableKey();
            return _types.GetOrAdd(key, _ => _factory.GetOrBuild(def));
        }
        
        public bool TryGet(string stableKey, out Type type) => _types.TryGetValue(stableKey, out type);
        
        public IReadOnlyDictionary<string, Type> Snapshot() => _types;
    }
}
