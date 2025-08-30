using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.TypeGeneration
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class GeneratedModelAttribute : Attribute
    {
        public string Name { get; }
        public string Version { get; }
        public string StableKey { get; }
        public GeneratedModelAttribute(string name, string version, string stableKey)
        {
            Name = name; Version = version; StableKey = stableKey;
        }
    }
}
