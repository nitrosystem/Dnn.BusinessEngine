using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.Caching
{
    public class CacheService : ICacheService
    {
        private readonly MemoryCache _cache = MemoryCache.Default;
        private readonly HashSet<string> _cacheKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly object _lock = new object();

        public async Task<T> GetOrCreateAsync<T>(string cacheKey, Func<Task<T>> factory, int? cacheTimeout = 20)
        {
            if (string.IsNullOrEmpty(cacheKey)) return await factory();

            var value = _cache.Get(cacheKey);
            if (value != null) return (T)value;

            // ایجاد مقدار در صورت نبودن در کش
            var result = await factory();

            var policy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddHours(cacheTimeout ?? 20)
            };

            _cache.Set(cacheKey, result, policy);

            lock (_lock)
            {
                _cacheKeys.Add(cacheKey);
            }

            return result;
        }

        public T Get<T>(string cacheKey)
        {
            if (string.IsNullOrEmpty(cacheKey)) return default;
            var value = _cache.Get(cacheKey);
            return value is T typed ? typed : default;
        }

        public void Set<T>(string cacheKey, T value, int? cacheTimeout = 20)
        {
            if (string.IsNullOrEmpty(cacheKey)) return;

            var policy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddHours(cacheTimeout ?? 20)
            };

            _cache.Set(cacheKey, value, policy);

            lock (_lock)
            {
                _cacheKeys.Add(cacheKey);
            }
        }

        public void Remove(string key)
        {
            if (string.IsNullOrEmpty(key)) return;

            _cache.Remove(key);

            lock (_lock)
            {
                _cacheKeys.Remove(key);
            }
        }

        public void RemoveByPrefix(string prefix)
        {
            if (string.IsNullOrEmpty(prefix)) return;

            List<string> keysToRemove;
            lock (_lock)
            {
                keysToRemove = _cacheKeys.Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
                lock (_lock)
                {
                    _cacheKeys.Remove(key);
                }
            }
        }

        public void ClearByPrefix(string prefix)
        {
            // همانند RemoveByPrefix ولی سریع‌تر و بدون چک مجدد
            RemoveByPrefix(prefix);
        }
    }
}
