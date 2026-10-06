using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts
{
    public interface ICacheService
    {
        Task<T> GetOrCreateAsync<T>(string cacheKey, Func<Task<T>> factory, int? cacheTimeout = 20);
        T GetOrCreate<T>(string cacheKey, Func<T> factory, int? cacheTimeout = 20);
        Task<IReadOnlyList<T>> GetOrCreateReadOnlyListAsync<T>(string cacheKey, Func<Task<IEnumerable<T>>> factory, int? cacheTimeout = 20);
        IReadOnlyList<T> GetOrCreateReadOnlyList<T>(string cacheKey, Func<IEnumerable<T>> factory, int? cacheTimeout = 20);
        T Get<T>(string cacheKey);
        void Set<T>(string cacheKey, T value, int? cacheTimeout = 20);
        void Remove(string key);
        void RemoveByPrefix(string prefix);
        void ClearByPrefix(string prefix);
        void ClearAll();
    }
}
