using System;

namespace NitroSystem.Dnn.BusinessEngine.Core.Reflection.TypeGeneration
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
