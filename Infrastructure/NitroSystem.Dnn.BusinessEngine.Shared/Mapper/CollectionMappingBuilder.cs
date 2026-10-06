using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Shared.Mapper
{
    public class CollectionMappingBuilder<TSource, TDestination> where TDestination : new()
    {
        //Instead of storing data, we store actions.
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
       
        public async Task<TDestination> BuildAsync(
            TSource source,
            Action<TSource, TDestination> afterMap = null
        )
        {
            if (source == null) return default;

            var dest = HybridMapper.Map<TSource, TDestination>(source);
            afterMap?.Invoke(source, dest);

            // Lazy map of all childs
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
}