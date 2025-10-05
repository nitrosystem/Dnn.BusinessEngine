using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NitroSystem.Dnn.BusinessEngine.Shared.Mapper
{
    public class ExpressionMapper<TSource, TDestination>
    {
        private static readonly ConcurrentDictionary<(Type, Type), Delegate> _mappingCache = new ConcurrentDictionary<(Type, Type), Delegate>();
        //private readonly HashSet<string> _ignoredProperties = new HashSet<string>();
         private static readonly Dictionary<(Type, Type), HashSet<string>> _ignoredProperties = 
            new Dictionary<(Type, Type), HashSet<string>>();

        private static Func<TSource, TDestination> CreateMapExpression()
        {
            var sourceParam = Expression.Parameter(typeof(TSource), "source");
            var bindings = new List<MemberBinding>();
            var key = (typeof(TSource), typeof(TDestination));
            var ignored = _ignoredProperties.TryGetValue(key, out var set) ? set : new HashSet<string>();

            foreach (var destProp in typeof(TDestination).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                         .Where(p => p.CanWrite && !ignored.Contains(p.Name)))
            {
                var sourceProp = typeof(TSource).GetProperty(destProp.Name, BindingFlags.Public | BindingFlags.Instance);
                if (sourceProp != null && sourceProp.CanRead && destProp.PropertyType.IsAssignableFrom(sourceProp.PropertyType))
                    bindings.Add(Expression.Bind(destProp, Expression.Property(sourceParam, sourceProp)));
            }

            foreach (var destField in typeof(TDestination).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                         .Where(f => !ignored.Contains(f.Name)))
            {
                var sourceField = typeof(TSource).GetField(destField.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (sourceField != null)
                    bindings.Add(Expression.Bind(destField, Expression.Field(sourceParam, sourceField)));
            }

            var body = Expression.MemberInit(Expression.New(typeof(TDestination)), bindings);
            return Expression.Lambda<Func<TSource, TDestination>>(body, sourceParam).Compile();
        }


        public static TDestination Map(TSource source)
        {
            if (source == null) return default;

            var key = (typeof(TSource), typeof(TDestination));
            var func = (Func<TSource, TDestination>)_mappingCache.GetOrAdd(key, _ => CreateMapExpression());
            var destination = func(source);

            return destination;
        }
    }
}
