using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Shared.Extensions
{
    public static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(
            this IDictionary<TKey, TValue> dict, TKey key)
        {
            return dict != null && dict.TryGetValue(key, out var value) ? value : default;
        }
    }
}
