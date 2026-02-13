using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Import
{
    public class ImportContext
    {
        private readonly Dictionary<string, object> _items = new();

        public ImportExportScope Scope { get; set; }
        public IUnitOfWork UnitOfWork { get; set; }
        public IReadOnlyDictionary<string, object> Items => _items;
        public Dictionary<string, object> DataTrack { get; set; }

        public void Set<T>(string key, T value)
        {
            _items[key] = value!;
        }

        public T Get<T>(string key)
        {
            if (!_items.TryGetValue(key, out var value))
                throw new Exception($"Export context key not found: {key}");

            if (typeof(T) == typeof(Guid))
                return (T)(object)Guid.Parse(value.ToString());

            if (value is not T typedValue)
                throw new Exception($"Export context key '{key}' is not of type {typeof(T).Name}");

            return typedValue;
        }

        public bool TryGet<T>(string key, out T value)
        {
            if (!_items.TryGetValue(key, out var v))
            {
                value = default;
                return false;
            }

            if (v != null)
            {
                if (typeof(T) == typeof(Guid))
                {
                    value = (T)(object)Guid.Parse(v.ToString());
                    return true;
                }

                if (typeof(T) == typeof(Guid?))
                {
                    value = (T)(object)Guid.Parse(v.ToString());
                    return true;
                }

                if (typeof(T) == typeof(int?))
                {
                    value = (T)(object)int.Parse(v.ToString());
                    return true;
                }

                if (typeof(T) == typeof(JObject))
                {
                    value = (T)(object)(JObject)v;
                    return true;
                }
            }

            value = (T)v;
            return false;
        }
    }
}
