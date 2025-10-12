using System;
using System.Reflection;

namespace NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.TypeLoader
{
    internal sealed class PluginLoader : MarshalByRefObject
    {
        internal Assembly Load(string assemblyPath, string typeFullName)
        {
            var assembly = Assembly.LoadFrom(assemblyPath);

            return assembly;
        }
    }
}
