using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Shared.Reflection
{
    public static class PropertyCopier<TSource, TTarget>
    {
        private static readonly List<PropertyInfo> Properties = typeof(TSource)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(prop =>
            {
                var targetProp = typeof(TTarget).GetProperty(prop.Name);
                return prop.CanRead
                       && targetProp != null
                       && targetProp.GetType() == prop.GetType()
                       && targetProp.CanWrite;
            })
            .ToList();

        public static void Copy(TSource source, TTarget destination)
        {
            if (source == null || destination == null)
                throw new ArgumentNullException("Source or destination cannot be null");

            foreach (var prop in Properties)
            {
                var targetProp = typeof(TTarget).GetProperty(prop.Name);
                targetProp?.SetValue(destination, prop.GetValue(source));
            }
        }
    }
}
