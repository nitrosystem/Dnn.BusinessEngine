using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.Mapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public static class HybridMapper
    {
        private static readonly Dictionary<(Type, Type), Delegate> _mappingCache = new();
        private static readonly Dictionary<(Type, Type), ExpressionMapper<object, object>> _expressionMappers = new();
        private static readonly Dictionary<(Type, Type), HashSet<string>> _ignoredProperties = new();
        private static readonly Dictionary<(Type, Type), List<Action<object, object>>> _beforeMaps = new();
        private static readonly Dictionary<(Type, Type), List<Action<object, object>>> _afterMaps = new();
        private static readonly Dictionary<(Type, Type), Func<object, object>> _genericMappers = new();

        public static void RegisterExpressionMapper<TSource, TDestination>(ExpressionMapper<TSource, TDestination> mapper)
        {
            _expressionMappers[(typeof(TSource), typeof(TDestination))] = mapper as ExpressionMapper<object, object>;
        }

        public static void Ignore<TSource, TDestination>(params string[] names)
        {
            var key = (typeof(TSource), typeof(TDestination));
            if (!_ignoredProperties.ContainsKey(key))
                _ignoredProperties[key] = new HashSet<string>();

            foreach (var name in names)
                _ignoredProperties[key].Add(name);
        }

        public static void BeforeMap<TSource, TDestination>(Action<TSource, TDestination> action)
        {
            var key = (typeof(TSource), typeof(TDestination));
            if (!_beforeMaps.ContainsKey(key))
                _beforeMaps[key] = new List<Action<object, object>>();

            _beforeMaps[key].Add((src, dest) => action((TSource)src, (TDestination)dest));
        }

        public static void AfterMap<TSource, TDestination>(Action<TSource, TDestination> action)
        {
            var key = (typeof(TSource), typeof(TDestination));
            if (!_afterMaps.ContainsKey(key))
                _afterMaps[key] = new List<Action<object, object>>();

            _afterMaps[key].Add((src, dest) => action((TSource)src, (TDestination)dest));
        }

        public static TDestination Map<TSource, TDestination>(TSource source)
            where TDestination : new()
        {
            if (source == null)
                return default;

            var key = (typeof(TSource), typeof(TDestination));

            if (_expressionMappers.TryGetValue(key, out var mapper))
                return (TDestination)mapper.Map(source);

            if (!_mappingCache.TryGetValue(key, out var del))
            {
                del = CreateMapExpression<TSource, TDestination>();
                _mappingCache[key] = del;
            }

            var func = (Func<TSource, TDestination>)del;
            var destination = func(source);

            if (_beforeMaps.TryGetValue(key, out var beforeList))
                beforeList.ForEach(action => action(source, destination));

            if (_afterMaps.TryGetValue(key, out var afterList))
                afterList.ForEach(action => action(source, destination));

            return destination;
        }

        public static TDestination MapWithConfig<TSource, TDestination>(TSource source, Action<TSource, TDestination> configAction)
            where TDestination : new()
        {
            var destination = Map<TSource, TDestination>(source);
            configAction?.Invoke(source, destination);
            return destination;
        }

        public static async Task<TDestination> MapWithConfigAsync<TSource, TDestination>(
            TSource source,
            Func<TSource, TDestination, Task> configAction)
            where TDestination : new()
        {
            var destination = Map<TSource, TDestination>(source);
            if (configAction != null)
                await configAction(source, destination);
            return destination;
        }

        public static TDestination MapWithChildCollection<TSource, TDestination, TChildSource, TChildDestination>(
            TSource source,
            Func<TSource, IEnumerable<TChildSource>> childSelector,
            Action<TDestination, IEnumerable<TChildDestination>> childAssigner
        ) 
            where TDestination : new()
            where TChildDestination : new()
        {
            if (source == null) return default;

            var destination = Map<TSource, TDestination>(source);

            var childSourceList = childSelector(source);
            if (childSourceList != null)
            {
                var mapper = GetOrCreateMapper<TChildSource, TChildDestination>();

                var childDestList = childSourceList
                    .Select(child => mapper(child))
                    .ToList();

                childAssigner(destination, childDestList);
            }

            return destination;
        }

        private static Func<TSource, TDestination> GetOrCreateMapper<TSource, TDestination>()
            where TDestination : new()
        {
            var key = (typeof(TSource), typeof(TDestination));

            if (_expressionMappers.TryGetValue(key, out var expressionMapper))
            {
                return source => (TDestination)expressionMapper.Map(source);
            }

            if (_genericMappers.TryGetValue(key, out var funcObj))
            {
                return source => (TDestination)funcObj(source);
            }

            // بساز و کش کن
            var func = CreateMapExpression<TSource, TDestination>();
            _genericMappers[key] = src => func((TSource)src);
            return func;
        }


        private static Func<TSource, TDestination> CreateMapExpression<TSource, TDestination>()
            where TDestination : new()
        {
            var sourceParam = Expression.Parameter(typeof(TSource), "source");
            var bindings = new List<MemberBinding>();

            var key = (typeof(TSource), typeof(TDestination));
            var ignored = _ignoredProperties.ContainsKey(key) ? _ignoredProperties[key] : new HashSet<string>();

            // --- Map Properties ---
            var destProperties = typeof(TDestination)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && !ignored.Contains(p.Name));

            foreach (var destProp in destProperties)
            {
                var sourceProp = typeof(TSource).GetProperty(destProp.Name, BindingFlags.Public | BindingFlags.Instance);
                if (sourceProp != null && sourceProp.CanRead && sourceProp.PropertyType == destProp.PropertyType)
                {
                    var bind = Expression.Bind(destProp, Expression.Property(sourceParam, sourceProp));
                    bindings.Add(bind);
                }
            }

            // --- Map Fields ---
            //var destFields = typeof(TDestination)
            //    .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            //    .Where(f => !ignored.Contains(f.Name));

            //foreach (var destField in destFields)
            //{
            //    var sourceField = typeof(TSource).GetField(destField.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            //    if (sourceField != null)
            //    {
            //        var bind = Expression.Bind(destField, Expression.Field(sourceParam, sourceField));
            //        bindings.Add(bind);
            //    }
            //}

            var body = Expression.MemberInit(Expression.New(typeof(TDestination)), bindings);
            var lambda = Expression.Lambda<Func<TSource, TDestination>>(body, sourceParam);
            return lambda.Compile();
        }

        public static TDestination MapSimpleWithDefaults<TSource, TDestination>(
            TSource source,
            Dictionary<string, object> defaultValues = null
        ) where TDestination : new()
        {
            var destination = new TDestination();
            var sourceProps = typeof(TSource).GetProperties();
            var destProps = typeof(TDestination).GetProperties();

            foreach (var destProp in destProps)
            {
                var sourceProp = sourceProps.FirstOrDefault(p =>
                    p.Name == destProp.Name &&
                    p.PropertyType == destProp.PropertyType);

                if (sourceProp != null && sourceProp.CanRead && destProp.CanWrite)
                {
                    var value = sourceProp.GetValue(source);
                    destProp.SetValue(destination, value);
                }
                else if (defaultValues != null && defaultValues.ContainsKey(destProp.Name))
                {
                    destProp.SetValue(destination, defaultValues[destProp.Name]);
                }
            }

            return destination;
        }

    }
}
