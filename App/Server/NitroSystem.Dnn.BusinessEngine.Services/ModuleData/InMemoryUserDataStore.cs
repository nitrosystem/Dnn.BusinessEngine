using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Mobile;
using DotNetNuke.UI.UserControls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NitroSystem.Dnn.BusinessEngine.App.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.App.Services.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.TypeLoader;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static NitroSystem.Dnn.BusinessEngine.App.Services.Delegates.Delegates;

namespace NitroSystem.Dnn.BusinessEngine.App.Services.ModuleData
{
    public class InMemoryUserDataStore : IUserDataStore
    {
        private readonly IModuleService _moduleService;
        private readonly ITypeLoaderFactory _typeLoaderFactory;
        private readonly IExpressionService _expressionService;

        private ConcurrentDictionary<string, Dictionary<Guid, ConcurrentDictionary<string, object>>> _store =
            new ConcurrentDictionary<string, Dictionary<Guid, ConcurrentDictionary<string, object>>>();

        private readonly ConcurrentDictionary<string, DateTime> _session =
            new ConcurrentDictionary<string, DateTime>();

        private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, object>> _moduleTemplateCache =
            new ConcurrentDictionary<Guid, ConcurrentDictionary<string, object>>();

        private readonly ConcurrentDictionary<Guid, ConcurrentBag<string>> _serverVariables =
             new ConcurrentDictionary<Guid, ConcurrentBag<string>>();

        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks =
            new ConcurrentDictionary<string, SemaphoreSlim>();

        public InMemoryUserDataStore(IModuleService moduleService, ITypeLoaderFactory typeLoaderFactory, IExpressionService expressionService)
        {
            _moduleService = moduleService;
            _typeLoaderFactory = typeLoaderFactory;
            _expressionService = expressionService;
        }

        public async Task<ConcurrentDictionary<string, object>> GetOrCreateModuleDataAsync(string connectionId, Guid moduleId, PortalSettings portalSettings)
        {
            var userModules = _store.GetOrAdd(connectionId, _ => new Dictionary<Guid, ConcurrentDictionary<string, object>>());
            var locker = _locks.GetOrAdd(connectionId, _ => new SemaphoreSlim(1, 1));

            await locker.WaitAsync();
            try
            {
                if (userModules.TryGetValue(moduleId, out var existing))
                    return existing;

                var clonedData = await GetClonedModuleData(moduleId, portalSettings);

                userModules[moduleId] = clonedData;

                return clonedData;
            }
            finally
            {
                locker.Release();
            }
        }

        private async Task<ConcurrentDictionary<string, object>> GetClonedModuleData(Guid moduleId, PortalSettings portalSettings)
        {
            if (!_moduleTemplateCache.TryGetValue(moduleId, out var originalModuleData))
            {
                _serverVariables.TryAdd(moduleId, new ConcurrentBag<string>());

                var moduleData = new ConcurrentDictionary<string, object>();

                var variables = await _moduleService.GetModuleVariables(moduleId, ModuleVariableScope.ServerSide);
                foreach (var variable in variables)
                {
                    if (variable.VariableType == "AppModel")
                    {
                        var type = _typeLoaderFactory.GetTypeFromAssembly(variable.ModelTypeRelativePath, variable.ModelTypeFullName, variable.ScenarioName, portalSettings);
                        var instance = Activator.CreateInstance(type);

                        moduleData[variable.VariableName] = instance;
                    }
                    else if (variable.VariableType == "AppModelList")
                    {
                        var type = _typeLoaderFactory.GetTypeFromAssembly(variable.ModelTypeRelativePath, variable.ModelTypeFullName, variable.ScenarioName, portalSettings);
                        var listType = typeof(List<>).MakeGenericType(type);
                        var emptyList = Activator.CreateInstance(listType);

                        moduleData[variable.VariableName] = emptyList;
                    }
                    else
                    {
                        moduleData[variable.VariableName] = TypeChecker.GetSystemTypeDefaultValue(variable.VariableType);

                        if (!string.IsNullOrWhiteSpace(variable.DefaultValue))
                        {
                            var setter = _expressionService.BuildDataSetter(variable.VariableName, moduleData);
                            setter(variable.DefaultValue);
                        }
                    }

                    if (variable.Scope == ModuleVariableScope.ServerSide) _serverVariables[moduleId].Add(variable.VariableName);
                }

                _moduleTemplateCache.TryAdd(moduleId, moduleData);
                originalModuleData = moduleData;
            }

            var clonedDict = new ConcurrentDictionary<string, object>();
            foreach (var kvp in originalModuleData)
            {
                if (kvp.Value is ICloneable cloneable)
                {
                    clonedDict[kvp.Key] = cloneable.Clone();
                }
                else
                {
                    clonedDict[kvp.Key] = kvp.Value;
                }
            }

            return clonedDict;
        }

        public ConcurrentDictionary<string, object> GetDataForClients(Guid moduleId, ConcurrentDictionary<string, object> moduleData)
        {
            if (!_serverVariables.TryGetValue(moduleId, out var excludedKeys))
                return new ConcurrentDictionary<string, object>();

            var filteredData = new ConcurrentDictionary<string, object>();

            foreach (var kvp in moduleData)
            {
                if (!excludedKeys.Contains(kvp.Key))
                    filteredData[kvp.Key] = kvp.Value;
            }

            return filteredData;
        }

        public async Task<ConcurrentDictionary<string, object>> UpdateModuleData(string connectionId, Guid moduleId, Dictionary<string, object> incomingData, PortalSettings portalSettings)
        {
            var moduleData = await GetOrCreateModuleDataAsync(connectionId, moduleId, PortalSettings.Current);

            var variables = await _moduleService.GetModuleVariables(moduleId, ModuleVariableScope.Global);
            foreach (var variable in variables.Where(v => moduleData.Keys.Contains(v.VariableName) && incomingData.Keys.Contains(v.VariableName)))
            {
                if (variable.VariableType == "AppModel")
                {
                    var type = _typeLoaderFactory.GetTypeFromAssembly(variable.ModelTypeRelativePath, variable.ModelTypeFullName, variable.ScenarioName, portalSettings);
                    var dict = incomingData[variable.VariableName] as JObject;
                    var data = dict.ToObject(type);

                    moduleData.TryUpdate(variable.VariableName, data, moduleData[variable.VariableName]);
                }
                else if (variable.VariableType == "AppModelList")
                {
                    var type = _typeLoaderFactory.GetTypeFromAssembly(variable.ModelTypeRelativePath, variable.ModelTypeFullName, variable.ScenarioName, portalSettings);
                    var dict = incomingData[variable.VariableName] as JArray;
                    var listType = typeof(List<>).MakeGenericType(type);
                    var data = dict.ToObject(listType);

                    moduleData.TryUpdate(variable.VariableName, data, moduleData[variable.VariableName]);
                }
                else
                {
                    moduleData.TryUpdate(variable.VariableName, incomingData[variable.VariableName], moduleData[variable.VariableName]);
                }
            }

            return moduleData;
        }

        public void Ping(string connectionId)
        {
            _session.AddOrUpdate(
                connectionId,
                DateTime.UtcNow,                 // مقدار اولیه اگر وجود نداشت
                (key, oldValue) => DateTime.UtcNow // مقدار جدید اگر وجود داشت
            );
        }

        public void CleanupOldConnections(TimeSpan timeout)
        {
            var now = DateTime.UtcNow;
            foreach (var kvp in _session.ToArray()) // ← گرفتن snapshot ایمن
            {
                if ((now - kvp.Value) > timeout)
                {
                    _session.TryRemove(kvp.Key, out _);
                    _store.TryRemove(kvp.Key, out _); // ← اگر null-safe نیست، بررسی کن
                }
            }
        }

        private bool TryParseVariableType(string typeName, out VariableType type)
        {
            return Enum.TryParse(typeName, ignoreCase: true, out type);
        }
    }
}
