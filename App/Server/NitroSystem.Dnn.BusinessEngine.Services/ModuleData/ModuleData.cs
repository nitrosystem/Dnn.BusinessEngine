using Newtonsoft.Json.Linq;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Common.Reflection;
using NitroSystem.Dnn.BusinessEngine.Common.Models.Shared;
using Newtonsoft.Json;
using System.Collections;
using NitroSystem.Dnn.BusinessEngine.Common.Helpers.Globals;
using Dapper;

namespace NitroSystem.Dnn.BusinessEngine.App.Services.ModuleData
{
    public class ModuleData : IModuleData
    {
        private readonly ConcurrentDictionary<string, object> _data = new ConcurrentDictionary<string, object>();

        public DateTime LastPing { get; set; } = DateTime.UtcNow;

        public IReadOnlyDictionary<string, object> GetAll() => _data;

        public object Get(string key)
        {
            if (_data.TryGetValue(key, out var value))
                return value;

            throw new KeyNotFoundException($"Key '{key}' not found.");
        }

        public bool TryGet(string key, out object value) => _data.TryGetValue(key, out value);

        public void Set(string key, object value)
        {
            if (_data.ContainsKey(key))
                _data.TryUpdate(key, value, _data[key]);
            else
                _data.TryAdd(key, value);
        }

        public void UpdateObjects(Dictionary<string, object> data)
        {
            foreach (var key in _data.Keys)
            {
                _data.TryUpdate(key, data[key], _data[key]);
            }
        }

        public void SetPageParams(string pageUrl) => Set("_PageParam", UrlHelper.ParsePageParameters(pageUrl));
    }
}