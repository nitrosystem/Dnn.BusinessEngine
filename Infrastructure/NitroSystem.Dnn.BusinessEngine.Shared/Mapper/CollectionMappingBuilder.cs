using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Shared.Mapper
{
    public class CollectionMappingBuilder<TSource, TDestination> where TDestination : new()
    {
        // به جای ذخیره‌ی داده، اکشن‌ها رو ذخیره می‌کنیم
        private readonly List<Func<TDestination, Task>> _childMappings = new List<Func<TDestination, Task>>();

        /// <summary>
        /// Adds an asynchronous child-mapping rule with parent-child key matching.
        /// Allows applying an async config action to each child destination.
        /// </summary>
        public void AddChildAsync<TChildSource, TChildDestination, TKey>(
            IEnumerable<TChildSource> source,
            Func<TDestination, TKey> parentKey,
            Func<TChildSource, TKey> childKey,
            Action<TDestination, IEnumerable<TChildDestination>> assign,
            Func<TChildSource, TChildDestination, Task> configAction = null
        )
            where TChildDestination : new()
        {
            var lookup = source.ToLookup(childKey); // Build child lookup once

            _childMappings.Add(async dest =>
            {
                // Extract the key of the current parent
                var key = parentKey(dest);

                // Fast O(1) lookup of matching children
                var matched = lookup[key];

                // Map collection asynchronously with optional per-item async config
                var mapped = await HybridMapper.MapCollectionAsync(
                    matched,
                    configAction
                );

                // Assign mapped children to parent
                assign(dest, mapped);
            });
        }

        public void AddChild<TChildSource, TChildDestination, TKey>(
            IEnumerable<TChildSource> source,
            Func<TDestination, TKey> parentKey,
            Func<TChildSource, TKey> childKey,
            Action<TDestination, IEnumerable<TChildDestination>> assign,
            Func<TChildSource, TChildDestination, Task> configAction = null
        )
            where TChildDestination : new()
        {
            var lookup = source.ToLookup(childKey);
            _childMappings.Add(async dest =>
            {
                var key = parentKey(dest);
                var matched = lookup[key]; // O(1) دسترسی
                var mapped = await HybridMapper.MapCollectionAsync<TChildSource, TChildDestination>(matched);
                assign(dest, mapped);
            });
        }

        //public async Task<TDestination> BuildAsync(TSource source)
        //{
        //    if (source == null) return default;

        //    var dest = HybridMapper.Map<TSource, TDestination>(source);

        //    // Childها موازی
        //    await Task.WhenAll(_childMappings.Select(map => map(dest)));

        //    return dest;
        //}

        //public async Task<IEnumerable<TDestination>> BuildAsync(IEnumerable<TSource> sources)
        //{
        //    if (sources == null) return Enumerable.Empty<TDestination>();

        //    // Parentها هم موازی
        //    return await Task.WhenAll(sources.Select(src => BuildAsync(src)));
        //}

        public async Task<TDestination> BuildAsync(
            TSource source,
            Action<TSource, TDestination> afterMap = null
        )
        {
            if (source == null) return default;

            var dest = HybridMapper.Map<TSource, TDestination>(source);
            afterMap?.Invoke(source, dest);

            // اجرای Lazy همه‌ی Childها
            foreach (var map in _childMappings)
                await map(dest);

            return dest;
        }

        public async Task<IEnumerable<TDestination>> BuildAsync(
            IEnumerable<TSource> sources,
            Action<TSource, TDestination> afterMap = null
        )
        {
            if (sources == null) return Enumerable.Empty<TDestination>();

            var results = new List<TDestination>();
            foreach (var src in sources)
                results.Add(await BuildAsync(src, afterMap));

            return results;
        }
    }


    //using System;
    //using System.Collections.Concurrent;
    //using System.Collections.Generic;
    //using System.Linq;
    //using System.Threading.Tasks;

    //namespace NitroSystem.Dnn.BusinessEngine.Shared.Mapper
    //{
    //    public class CollectionMappingBuilder<TSource, TDestination> where TDestination : new()
    //    {
    //        private static ConcurrentDictionary<Action<TDestination, IEnumerable<object>>, IEnumerable<object>> _childs = new
    //            ConcurrentDictionary<Action<TDestination, IEnumerable<object>>, IEnumerable<object>>();

    //        public void AddChild<TChildSource, TChildDestination>(
    //            IEnumerable<TChildSource> source,
    //            Action<TDestination, IEnumerable<object>> assign
    //        )
    //            where TChildDestination : new()
    //        {
    //            var dest = HybridMapper.MapCollection<TChildSource, TChildDestination>(source);
    //            _childs.TryAdd(assign, source.Cast<object>());
    //        }

    //        public async Task AddChildAsync<TChildSource, TChildDestination>(
    //            IEnumerable<TChildSource> source,
    //            Action<TDestination, IEnumerable<object>> assign,
    //            Func<TChildSource, TChildDestination, Task> configAction = null)
    //            where TChildDestination : new()
    //        {
    //            var dest = await HybridMapper.MapCollectionAsync<TChildSource, TChildDestination>(source, configAction);

    //            _childs.TryAdd(assign, source.Cast<object>());
    //        }

    //        public TDestination build(TSource source)
    //        {
    //            if (source == null) return default;

    //            var dest = HybridMapper.Map<TSource, TDestination>(source);

    //            foreach (var child in _childs)
    //            {
    //                var assign = child.Key;
    //                assign(dest, child.Value);
    //            }

    //            return dest;
    //        }

    //        public IEnumerable<TDestination> build(IEnumerable<TSource> sources)
    //        {
    //            if (sources == null) return default;

    //            var dest = new List<TDestination>();

    //            foreach (var source in sources)
    //            {
    //                dest.Add(build(source));
    //            }

    //            return dest;
    //        }
    //    }
    //}
}