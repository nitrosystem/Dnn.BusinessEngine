using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Reflection.TypeLoader
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
