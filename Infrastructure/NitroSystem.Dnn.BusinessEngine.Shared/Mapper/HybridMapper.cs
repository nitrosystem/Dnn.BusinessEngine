using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Shared.Mapper
{
    public static class HybridMapper
    {
        private static readonly ConcurrentDictionary<(Type, Type), Delegate> _mappingCache =
            new ConcurrentDictionary<(Type, Type), Delegate>();
        private static readonly ConcurrentDictionary<(Type, Type), HashSet<string>> _ignoredProperties =
            new ConcurrentDictionary<(Type, Type), HashSet<string>>();
        private static readonly ConcurrentDictionary<(Type, Type), List<Action<object, object>>> _beforeMaps =
            new ConcurrentDictionary<(Type, Type), List<Action<object, object>>>();
        private static readonly ConcurrentDictionary<(Type, Type), List<Action<object, object>>> _afterMaps =
            new ConcurrentDictionary<(Type, Type), List<Action<object, object>>>();

        #region Configuration

        public static void Ignore<TSource, TDestination>(params string[] names)
        {
            var key = (typeof(TSource), typeof(TDestination));
            var set = _ignoredProperties.GetOrAdd(key, _ => new HashSet<string>());
            foreach (var name in names) set.Add(name);
        }

        public static void BeforeMap<TSource, TDestination>(Action<TSource, TDestination> action)
        {
            var key = (typeof(TSource), typeof(TDestination));
            var list = _beforeMaps.GetOrAdd(key, _ => new List<Action<object, object>>());
            list.Add((src, dest) => action((TSource)src, (TDestination)dest));
        }

        public static void AfterMap<TSource, TDestination>(Action<TSource, TDestination> action)
        {
            var key = (typeof(TSource), typeof(TDestination));
            var list = _afterMaps.GetOrAdd(key, _ => new List<Action<object, object>>());
            list.Add((src, dest) => action((TSource)src, (TDestination)dest));
        }

        #endregion

        #region Map Methods

        public static TDestination Map<TSource, TDestination>(TSource source)
            where TDestination : new()
        {
            if (source == null) return default;

            var key = (typeof(TSource), typeof(TDestination));
            var func = (Func<TSource, TDestination>)_mappingCache
                .GetOrAdd(key, _ => CreateMapExpression<TSource, TDestination>());

            var destination = func(source);

            if (_beforeMaps.TryGetValue(key, out var beforeList))
                beforeList.ForEach(a => a(source, destination));

            if (_afterMaps.TryGetValue(key, out var afterList))
                afterList.ForEach(a => a(source, destination));

            //AutoMapChildren(source, destination);
            return destination;
        }

        public static async Task<TDestination> MapAsync<TSource, TDestination>(
            TSource source,
            Func<TSource, TDestination, Task> configAction = null)
            where TDestination : new()
        {
            var destination = Map<TSource, TDestination>(source);
            if (configAction != null) await configAction(source, destination).ConfigureAwait(false);
            return destination;
        }

        public static async Task<TDestination> MapWithConfigAsync<TSource, TDestination>(
            TSource source,
            Func<TSource, TDestination, Task> configAction)
            where TDestination : new()
        {
            var destination = Map<TSource, TDestination>(source);
            if (configAction != null)
                await configAction(source, destination).ConfigureAwait(false);
            return destination;
        }

        public static TDestination MapWithConfig<TSource, TDestination>(
            TSource source,
            Action<TSource, TDestination> configAction)
            where TDestination : new()
        {
            var destination = Map<TSource, TDestination>(source);
            configAction?.Invoke(source, destination);
            return destination;
        }

        public static IEnumerable<TDestination> MapCollection<TSource, TDestination>(
            IEnumerable<TSource> sources,
            Action<TSource, TDestination> afterMap = null)
            where TDestination : new()
        {
            if (sources == null) return Enumerable.Empty<TDestination>();

            // اگر afterMap پاس داده بشه روی هر آیتم اعمال میشه
            return sources.Select(src =>
            {
                var dest = Map<TSource, TDestination>(src);
                afterMap?.Invoke(src, dest);
                return dest;
            });
        }

        public static async Task<IEnumerable<TDestination>> MapCollectionAsync<TSource, TDestination>
            (IEnumerable<TSource> sources,
            Func<TSource, TDestination, Task> configAction = null)
            where TDestination : new()
        {
            if (sources == null)
                return Enumerable.Empty<TDestination>();

            var results = new List<TDestination>();

            foreach (var src in sources)
                results.Add(await MapAsync(src, configAction).ConfigureAwait(false));

            return results;
        }
       
        /// <summary>
        /// Map یک parent و لیست children به parent با استفاده از childSelector
        /// </summary>
        public static TDestination MapWithChildren<TSource, TDestination, TChildSource, TChildDestination>(
            TSource source,
            IEnumerable<TChildSource> children,
            Action<TDestination, IEnumerable<TChildDestination>> assignChildren,
            Action<TSource, TDestination> moreAssigns = null)
            where TDestination : new()
            where TChildDestination : new()
        {
            if (source == null) return default;

            // Map parent
            var parentDest = Map<TSource, TDestination>(source);

            if (children != null)
            {
                // Map لیست children
                var childDestList = MapCollection<TChildSource, TChildDestination>(children);

                // Assign به parent
                assignChildren(parentDest, childDestList);
            }

            return parentDest;
        }

        /// <summary>
        /// ✅ ترکیب Parent/Child : والدها را مپ می‌کند، فرزندان مرتبط را پیدا و به آن‌ها ضمیمه می‌کند.
        /// </summary>
        public static IEnumerable<TParentDest> MapWithChildren<TParentSrc, TParentDest, TChildSrc, TChildDest>(
            IEnumerable<TParentSrc> parents,
            IEnumerable<TChildSrc> children,
            Func<TParentSrc, object> parentKeySelector,
            Func<TChildSrc, object> childKeySelector,
            Action<TParentDest, IEnumerable<TChildDest>> assignChildren,
            Action<TParentSrc, TParentDest> moreAssigns = null)
            where TParentDest : new()
            where TChildDest : new()
        {
            //if (parents == null) yield return Enumerable.Empty<TParentDest>();

            // Lookup برای فرزندان
            var lookup = (children ?? Enumerable.Empty<TChildSrc>())
                .GroupBy(childKeySelector)
                .ToDictionary(g => g.Key,
                              g => g.Select(Map<TChildSrc, TChildDest>).ToList());

            foreach (var parent in parents)
            {
                var pDest = Map<TParentSrc, TParentDest>(parent);
                var key = parentKeySelector(parent);
                if (key != null && lookup.TryGetValue(key, out var cList))
                    assignChildren(pDest, cList);

                yield return pDest;
            }
        }

        #endregion

        #region Private Helpers

        private static Func<TSource, TDestination> CreateMapExpression<TSource, TDestination>()
            where TDestination : new()
        {
            var sourceParam = Expression.Parameter(typeof(TSource), "source");
            var bindings = new List<MemberBinding>();
            var key = (typeof(TSource), typeof(TDestination));
            var ignored = _ignoredProperties.TryGetValue(key, out var set) ? set : new HashSet<string>();

            foreach (var destProp in typeof(TDestination).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                         .Where(p => p.CanWrite && !ignored.Contains(p.Name)))
            {
                var sourceProp = typeof(TSource).GetProperty(destProp.Name, BindingFlags.Public | BindingFlags.Instance);
                if (sourceProp != null && sourceProp.CanRead &&
                    destProp.PropertyType.IsAssignableFrom(sourceProp.PropertyType))
                {
                    bindings.Add(Expression.Bind(destProp, Expression.Property(sourceParam, sourceProp)));
                }
            }

            foreach (var destField in typeof(TDestination).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                         .Where(f => !ignored.Contains(f.Name)))
            {
                var sourceField = typeof(TSource).GetField(destField.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (sourceField != null && destField.FieldType.IsAssignableFrom(sourceField.FieldType))
                    bindings.Add(Expression.Bind(destField, Expression.Field(sourceParam, sourceField)));
            }

            var body = Expression.MemberInit(Expression.New(typeof(TDestination)), bindings);
            return Expression.Lambda<Func<TSource, TDestination>>(body, sourceParam).Compile();
        }

        private static object MapDynamic(object source, Type destinationType)
        {
            if (source == null) return null;
            var method = typeof(HybridMapper).GetMethod(nameof(Map), BindingFlags.Public | BindingFlags.Static);
            var generic = method.MakeGenericMethod(source.GetType(), destinationType);
            return generic.Invoke(null, new object[] { source });
        }

        #endregion
    }
}