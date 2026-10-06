using System;
using System.Collections.Concurrent;

namespace NitroSystem.Dnn.BusinessEngine.Core.Attributes
{
    public class AttributeCache
    {
        private static readonly ConcurrentDictionary<Type, object[]> _cache = new();
        
        public static AttributeCache Instance { get { return new AttributeCache(); } }

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
