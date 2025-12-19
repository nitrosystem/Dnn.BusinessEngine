using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Newtonsoft.Json.Linq;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Core.Reflection.TypeLoader;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Caching;

namespace NitroSystem.Dnn.BusinessEngine.App.DataService.ModuleData
{
    public class InMemoryUserDataStore : IUserDataStore
    {
        private readonly ICacheService _cacheService;
        private readonly IExpressionService _expressionService;
        private readonly ITypeLoaderFactory _typeLoaderFactory;
        private readonly IModuleService _moduleService;

        private readonly ConcurrentDictionary<string, Dictionary<Guid, ConcurrentDictionary<string, object>>> _store = new();
        private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, object>> _moduleTemplateCache = new();
        private readonly ConcurrentDictionary<Guid, ConcurrentBag<string>> _serverVariables = new();
        private readonly ConcurrentDictionary<Guid, ConcurrentBag<string>> _clientVariables = new();
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();
        public InMemoryUserDataStore(ICacheService cacheService, IModuleService moduleService, ITypeLoaderFactory typeLoaderFactory, IExpressionService expressionService)
        {
            _cacheService = cacheService;
            _moduleService = moduleService;
            _typeLoaderFactory = typeLoaderFactory;
            _expressionService = expressionService;
        }

        public async Task<ConcurrentDictionary<string, object>> GetOrCreateModuleDataAsync(string connectionId, Guid moduleId, string basePath)
        {
            var userModules = _store.GetOrAdd(connectionId, _ => new Dictionary<Guid, ConcurrentDictionary<string, object>>());
            var lockId = connectionId + moduleId;
            var locker = _locks.GetOrAdd(lockId, _ => new SemaphoreSlim(1, 1));

            await locker.WaitAsync();
            try
            {
                if (userModules.TryGetValue(moduleId, out var existing))
                    return existing;

                var clonedData = await GetClonedModuleData(moduleId, basePath);

                userModules[moduleId] = clonedData;

                return clonedData;
            }
            finally
            {
                locker.Release();
            }
        }

        public ConcurrentDictionary<string, object> GetDataForClients(string connectionId, Guid moduleId)
        {
            var result = new ConcurrentDictionary<string, object>();

            if (_store.TryGetValue(connectionId, out var usersData))
            {
                if (usersData.TryGetValue(moduleId, out var moduleData))
                {
                    _clientVariables.TryGetValue(moduleId, out var clientVariable);

                    if (_serverVariables.TryGetValue(moduleId, out var excludedKeys))
                    {
                        foreach (var kvp in moduleData)
                        {
                            if (!excludedKeys.Contains(kvp.Key))
                                result.TryAdd(kvp.Key, kvp.Value);
                        }
                    }
                }
            }

            CleanClientVariableData(connectionId, moduleId);

            return result;
        }

        public async Task<ConcurrentDictionary<string, object>> UpdateModuleData(string connectionId, Guid moduleId, Dictionary<string, object> incomingData, string basePath)
        {
            var moduleData = await GetOrCreateModuleDataAsync(connectionId, moduleId, basePath);

            var variables = await _moduleService.GetVariables(moduleId, ModuleVariableScope.Global, ModuleVariableScope.ServerSide);
            foreach (var variable in variables.Where(v => moduleData.Keys.Contains(v.VariableName) && incomingData.Keys.Contains(v.VariableName)))
            {
                var value = incomingData[variable.VariableName];
                if (value != null && variable.VariableType != "string" && string.IsNullOrEmpty(value.ToString()))
                    value = null;

                if (variable.VariableType == "AppModel" && value != null && value.GetType() == typeof(JObject))
                {
                    var type = _typeLoaderFactory.GetTypeFromAssembly(variable.ModelTypeRelativePath, variable.ModelTypeFullName, variable.ScenarioName, basePath);
                    var json = incomingData[variable.VariableName].ToString();
                    value = Newtonsoft.Json.JsonConvert.DeserializeObject(json, type);
                }
                else if (variable.VariableType == "AppModelList" && value != null && value.GetType() == typeof(JArray))
                {
                    var type = _typeLoaderFactory.GetTypeFromAssembly(variable.ModelTypeRelativePath, variable.ModelTypeFullName, variable.ScenarioName, basePath);
                    var listType = typeof(List<>).MakeGenericType(type);
                    var json = incomingData[variable.VariableName].ToString();
                    value = Newtonsoft.Json.JsonConvert.DeserializeObject(json, listType);
                }

                moduleData.TryUpdate(variable.VariableName, value, moduleData[variable.VariableName]);
            }

            return moduleData;
        }

        public void DisconnectUser(string connectionId, Guid moduleId)
        {
            if (_store.TryGetValue(connectionId, out var modules))
            {
                modules.Remove(moduleId);
                if (modules.Count == 0) _store.TryRemove(connectionId, out _);
            }
        }

        private async Task<ConcurrentDictionary<string, object>> GetClonedModuleData(Guid moduleId, string basePath)
        {
            var checkVariableCaching = _cacheService.Get<bool>("BE_Modules_Variables_IsClearCache");
            if (!checkVariableCaching || !_moduleTemplateCache.TryGetValue(moduleId, out var originalModuleData))
            {
                _serverVariables.TryAdd(moduleId, new ConcurrentBag<string>());
                _clientVariables.TryAdd(moduleId, new ConcurrentBag<string>());

                var moduleData = new ConcurrentDictionary<string, object>();

                var variables = await _moduleService.GetVariables(moduleId, ModuleVariableScope.Global, ModuleVariableScope.ServerSide);
                foreach (var variable in variables)
                {
                    if (variable.VariableType == "AppModel")
                    {
                        var type = _typeLoaderFactory.GetTypeFromAssembly(variable.ModelTypeRelativePath, variable.ModelTypeFullName, variable.ScenarioName, basePath);
                        var instance = Activator.CreateInstance(type);

                        moduleData[variable.VariableName] = instance;
                    }
                    else if (variable.VariableType == "AppModelList")
                    {
                        var type = _typeLoaderFactory.GetTypeFromAssembly(variable.ModelTypeRelativePath, variable.ModelTypeFullName, variable.ScenarioName, basePath);
                        var listType = typeof(List<>).MakeGenericType(type);
                        var emptyList = Activator.CreateInstance(listType);

                        moduleData[variable.VariableName] = emptyList;
                    }
                    else
                    {
                        moduleData[variable.VariableName] = null;// TypeChecker.GetSystemTypeDefaultValue(variable.VariableType);

                        if (!string.IsNullOrWhiteSpace(variable.DefaultValue))
                        {
                            var setter = _expressionService.BuildDataSetter(variable.VariableName, moduleData);
                            setter(variable.DefaultValue);
                        }
                    }

                    if (variable.Scope == ModuleVariableScope.ClientSide)
                        _clientVariables[moduleId].Add(variable.VariableName);
                    else if (variable.Scope == ModuleVariableScope.ServerSide)
                        _serverVariables[moduleId].Add(variable.VariableName);
                }

                _moduleTemplateCache.TryAdd(moduleId, moduleData);
                originalModuleData = moduleData;

                _cacheService.Set<bool>("BE_Modules_Variables_IsClearCache", true);
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

        private void CleanClientVariableData(string connectionId, Guid moduleId)
        {
            if (_store.TryGetValue(connectionId, out var usersData))
            {
                if (usersData.TryGetValue(moduleId, out var moduleData))
                {
                    _clientVariables.TryGetValue(moduleId, out var clientVariable);
                    foreach (var key in clientVariable)
                    {
                        if (moduleData.TryGetValue(key, out var data)) 
                            moduleData.TryUpdate(key, null, data);
                    }
                }
            }
        }

        public static object ConvertOrNull(string value, string typeName)
        {
            if (typeName == null)
                throw new ArgumentNullException(nameof(typeName));

            typeName = typeName.ToLowerInvariant();

            // string استثناء است
            if (typeName == "string")
                return value ?? string.Empty;

            // مقادیر خالی برای تایپ‌های غیر string
            if (string.IsNullOrWhiteSpace(value))
                return null;

            try
            {
                return typeName switch
                {
                    "int" => int.TryParse(value, out var i) ? i : null,
                    "long" => long.TryParse(value, out var l) ? l : null,
                    "double" => double.TryParse(value, out var d) ? d : null,
                    "decimal" => decimal.TryParse(value, out var m) ? m : null,
                    "bool" => bool.TryParse(value, out var b) ? b : null,
                    "datetime" => DateTime.TryParse(value, out var dt) ? dt : null,

                    _ => null
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
