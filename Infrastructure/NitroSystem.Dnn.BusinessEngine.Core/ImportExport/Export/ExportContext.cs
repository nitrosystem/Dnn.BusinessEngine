using System;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Export
{
    public class ExportContext
    {
        private readonly Dictionary<string, object> _items = new();

        private static class ExportContextKeys
        {
            public const string ScenarioId = "ScenarioId";
            public const string EntityId = "EntityId";
            public const string AppModelId = "AppModelId";
            public const string ServiceId = "ServiceId";
            public const string ModuleId = "ModuleId";
            public const string FieldId = "FieldId";
            public const string ActionId = "ActionId";
            public const string GenerateEntityScripts = "GenerateEntityScripts";
        }

        private static readonly HashSet<string> _keys = new()
        {
            ExportContextKeys.ScenarioId,
            ExportContextKeys.EntityId,
            ExportContextKeys.AppModelId,
            ExportContextKeys.ServiceId,
            ExportContextKeys.ModuleId,
            ExportContextKeys.FieldId,
            ExportContextKeys.ActionId,
            ExportContextKeys.GenerateEntityScripts
        };

        public ImportExportScope Scope { get; set; }
        public IReadOnlyDictionary<string, object> Items => _items;

        public void Set<T>(string key, T value)
        {
            if (!IsValid(key))
                throw new Exception($"Invalid export context key: {key}");

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

        private bool IsValid(string key) => _keys.Contains(key);
    }
}
