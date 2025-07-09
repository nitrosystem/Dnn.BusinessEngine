using NitroSystem.Dnn.BusinessEngine.Core.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.Cashing
{
    public interface ICacheService
    {
        Task<T> GetOrCreate<T>(string cacheKey, Func<Task<T>> factory, int? cacheTimeout = 20);
        T Get<T>(string key);
        void Set<T>(string cacheKey, T value, int? cacheTimeout = 20);
        void Remove(string key);
        void RemoveByPrefix(string prefix);
        void ClearByPrefix(string prefix);
    }
}
