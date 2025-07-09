using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetNuke.Common.Utilities;
using NitroSystem.Dnn.BusinessEngine.Core.Contract;
using Newtonsoft.Json.Linq;

namespace NitroSystem.Dnn.BusinessEngine.Core.Cashing
{
    public class CacheServiceBase : ICacheService
    {
        private readonly HashSet<string> _cacheKeys = new();

        public async Task<T> GetOrCreate<T>(string cacheKey, Func<Task<T>> factory, int? cacheTimeout = 20)
        {
            var value = DataCache.GetCache<T>(cacheKey);
            if (value != null)
                return value;
            else
            {
                value = await factory();

                DataCache.SetCache(cacheKey, value, TimeSpan.FromHours(cacheTimeout ?? 20));

                lock (_cacheKeys)
                {
                    _cacheKeys.Add(cacheKey);
                }
            }

            return value;
        }

        public T Get<T>(string cacheKey)
        {
            return DataCache.GetCache<T>(cacheKey);
        }

        public void Set<T>(string cacheKey, T value, int? cacheTimeout = 20)
        {
            DataCache.SetCache(cacheKey, value, TimeSpan.FromHours(cacheTimeout ?? 20));

            lock (_cacheKeys)
            {
                _cacheKeys.Add(cacheKey);
            }
        }

        public void Remove(string key)
        {
            lock (_cacheKeys)
            {
                DataCache.RemoveCache(key);
                _cacheKeys.Remove(key.ToString());
            }
        }

        public void RemoveByPrefix(string prefix)
        {
            lock (_cacheKeys)
            {
                var keysToRemove = _cacheKeys.Where(k => k.StartsWith(prefix)).ToList();
                foreach (var key in keysToRemove)
                {
                    DataCache.RemoveCache(key);
                    _cacheKeys.Remove(key);
                }
            }
        }

        public void ClearByPrefix(string prefix)
        {
            DataCache.ClearCache(prefix);
        }
    }
}
