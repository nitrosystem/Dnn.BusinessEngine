using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Shared.Mapper
{
    public static class HybridMapper
    {
        private static readonly ConcurrentDictionary<(Type, Type), Delegate> _mappingCache = new();
        private static readonly ConcurrentDictionary<(Type, Type), List<Action<object>>> _beforeMaps = new();
        private static readonly ConcurrentDictionary<(Type, Type), List<Action<object, object>>> _afterMaps = new();

        #region Configuration

        public static void BeforeMap<TSource, TDestination>(Action<TSource> action)
        {
            var key = (typeof(TSource), typeof(TDestination));
            var list = _beforeMaps.GetOrAdd(key, _ => new List<Action<object>>());
            lock (list)  // lock just for Add
            {
                list.Add((src) => action((TSource)src));
            }
        }

        public static void AfterMap<TSource, TDestination>(Action<TSource, TDestination> action)
        {
            var key = (typeof(TSource), typeof(TDestination));
            var list = _afterMaps.GetOrAdd(key, _ => new List<Action<object, object>>());
            lock (list)  // lock just for Add
            {
                list.Add((src, dest) => action((TSource)src, (TDestination)dest));
            }
        }

        #endregion

        #region Map Methods

        public static TDestination Map<TSource, TDestination>(TSource source)
            where TDestination : new()
        {
            if (source == null) return default;

            var key = (typeof(TSource), typeof(TDestination));
            var func = (Func<TSource, TDestination>)_mappingCache.GetOrAdd(key, _ => CreateMapExpression<TSource, TDestination>());

            // Before map - snapshot pattern
            if (_beforeMaps.TryGetValue(key, out var beforeList))
            {
                Action<object>[] snapshot;
                lock (beforeList) { snapshot = beforeList.ToArray(); }
                foreach (var item in snapshot) item(source);
            }

            // Map
            var destination = func(source);

            // After map - snapshot pattern
            if (_afterMaps.TryGetValue(key, out var afterList))
            {
                Action<object, object>[] snapshot;
                lock (afterList) { snapshot = afterList.ToArray(); }
                foreach (var item in snapshot) item(source, destination);
            }

            return destination;
        }

        public static TDestination Map<TSource, TDestination>(
           TSource source,
           Action<TSource, TDestination> configAction)
           where TDestination : new()
        {
            var destination = Map<TSource, TDestination>(source);

            configAction?.Invoke(source, destination);

            return destination;
        }
        public static Task<TDestination> MapAsync<TSource, TDestination>(
            TSource source,
            Func<TSource, TDestination, Task> configAction = null)
            where TDestination : new()
        {
            var destination = Map<TSource, TDestination>(source);

            if (configAction == null) return Task.FromResult(destination);

            return AwaitConfig(destination, configAction(source, destination));

            static async Task<TDestination> AwaitConfig(TDestination dest, Task configTask)
            {
                await configTask.ConfigureAwait(false);
                return dest;
            }
        }

        public static IReadOnlyList<TDestination> MapCollection<TSource, TDestination>(
            IEnumerable<TSource> sources,
            Action<TSource, TDestination> configAction = null)
            where TDestination : new()
        {
            if (sources == null) return Array.Empty<TDestination>();

            //If an afterMap is passed, it will be applied to each item.
            var result = sources is ICollection<TSource> coll
                ? new List<TDestination>(coll.Count)
                : new List<TDestination>();

            foreach (var src in sources)
            {
                var dest = Map<TSource, TDestination>(src);
                configAction?.Invoke(src, dest);
                result.Add(dest);
            }

            return result;
        }

        public static async Task<IReadOnlyList<TDestination>> MapCollectionAsync<TSource, TDestination>
            (IEnumerable<TSource> sources,
            Func<TSource, TDestination, Task> configAction = null)
            where TDestination : new()
        {
            if (sources == null) return Array.Empty<TDestination>();

            var results = new List<TDestination>();

            foreach (var src in sources)
                results.Add(await MapAsync(src, configAction).ConfigureAwait(false));

            return results;
        }

        /// <summary>
        /// Map a parent and list of children to parent using childSelector
        /// </summary>
        public static TDestination MapWithChildren<TSource, TDestination, TChildSource, TChildDestination>(
            TSource source,
            IEnumerable<TChildSource> children,
            Action<TDestination, IReadOnlyList<TChildDestination>> assignChildren,
            Action<TSource, TDestination> moreAssigns = null)
            where TDestination : new()
            where TChildDestination : new()
        {
            if (source == null) return default;

            // Map parent
            var parentDest = Map<TSource, TDestination>(source);

            if (children != null)
            {
                // Map list children
                var childDestList = MapCollection<TChildSource, TChildDestination>(children);

                // Assign to parent
                assignChildren(parentDest, childDestList);
            }

            moreAssigns?.Invoke(source, parentDest);

            return parentDest;
        }

        /// <summary>
        ///  Combine Parent/Child: Maps parents, finds related children, and attaches them.
        /// </summary>
        public static IReadOnlyList<TParentDest> MapWithChildren<TParentSrc, TParentDest, TChildSrc, TChildDest>(
            IEnumerable<TParentSrc> parents,
            IEnumerable<TChildSrc> children,
            Func<TParentSrc, object> parentKeySelector,
            Func<TChildSrc, object> childKeySelector,
            Action<TParentDest, IReadOnlyList<TChildDest>> assignChildren,
            Action<TParentSrc, TParentDest> moreAssigns = null)
            where TParentDest : new()
            where TChildDest : new()
        {
            if (parents == null) return Array.Empty<TParentDest>();

            // Lookup for children
            var lookup = (children ?? Enumerable.Empty<TChildSrc>()).Where(c => childKeySelector(c) != null)
                .GroupBy(childKeySelector)
                .ToDictionary(g => g.Key,
                              g => g.Select(Map<TChildSrc, TChildDest>).ToList());

            var result = new List<TParentDest>();

            foreach (var parent in parents)
            {
                var pDest = Map<TParentSrc, TParentDest>(parent);
                var key = parentKeySelector(parent);
                if (key != null && lookup.TryGetValue(key, out var cList))
                    assignChildren(pDest, cList);

                moreAssigns?.Invoke(parent, pDest);

                result.Add(pDest);
            }

            return result;
        }

        #endregion

        #region Private Helpers

        private static Func<TSource, TDestination> CreateMapExpression<TSource, TDestination>()
            where TDestination : new()
        {
            var sourceParam = Expression.Parameter(typeof(TSource), "source");
            var bindings = new List<MemberBinding>();

            foreach (var destProp in typeof(TDestination)
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(p => p.CanWrite))
            {
                var sourceProp = typeof(TSource).GetProperty(destProp.Name, BindingFlags.Public | BindingFlags.Instance);
                if (sourceProp != null && sourceProp.CanRead &&
                    destProp.PropertyType.IsAssignableFrom(sourceProp.PropertyType))
                {
                    bindings.Add(Expression.Bind(destProp, Expression.Property(sourceParam, sourceProp)));
                }
            }

            var body = Expression.MemberInit(Expression.New(typeof(TDestination)), bindings);
            return Expression.Lambda<Func<TSource, TDestination>>(body, sourceParam).Compile();
        }

        #endregion
    }
}