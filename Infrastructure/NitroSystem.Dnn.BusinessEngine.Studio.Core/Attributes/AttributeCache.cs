using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.Attributes
{
    public class AttributeCache
    {
        public static AttributeCache Instance { get { return new AttributeCache(); } }

        private static readonly ConcurrentDictionary<Type, object[]> _cache = new();

        public static object[] GetAttributes<T>()
        {
            Type type = typeof(T);
            return _cache.GetOrAdd(type, t => t.GetCustomAttributes(false));
        }

        public TAttribute GetAttribute<T, TAttribute>() where T : class
                                                where TAttribute : Attribute
        {
            var attributes = Attribute.GetCustomAttributes(typeof(T), typeof(TAttribute));
            if (attributes.Length > 0)
            {
                return (TAttribute)attributes[0];
            }

            return null;
        }
    }
}
