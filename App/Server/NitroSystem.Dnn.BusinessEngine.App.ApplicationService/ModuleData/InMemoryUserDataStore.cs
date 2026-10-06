using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Newtonsoft.Json.Linq;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Core.Reflection.TypeLoader;

namespace NitroSystem.Dnn.BusinessEngine.App.ApplicationService.ModuleData
{
    public class InMemoryUserDataStore : IUserDataStore
    {
        private readonly ICacheService _cacheService;
        private readonly ITypeLoaderFactory _typeLoaderFactory;
        private readonly IModuleVariableService _moduleVariableService;

        private readonly ConcurrentDictionary<string, Dictionary<Guid, ConcurrentDictionary<string, object>>> _store = new();
        private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, object>> _moduleTemplateCache = new();
        private readonly ConcurrentDictionary<Guid, ConcurrentBag<string>> _serverVariables = new();
        private readonly ConcurrentDictionary<Guid, ConcurrentBag<string>> _clientVariables = new();
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();
        private readonly IReadOnlyList<string> _systemVariables = new List<string>() { "_CurrentUserId", "_ServiceResult", "_PageParam" };

        public InMemoryUserDataStore(
            ICacheService cacheService,
            ITypeLoaderFactory typeLoaderFactory,
            IModuleVariableService moduleVariableService)
        {
            _cacheService = cacheService;
            _typeLoaderFactory = typeLoaderFactory;
            _moduleVariableService = moduleVariableService;
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
                    _serverVariables.TryGetValue(moduleId, out var servertVariables);
                    servertVariables = servertVariables ?? new ConcurrentBag<string>();

                    foreach (var kvp in moduleData)
                    {
                        if (!servertVariables.Contains(kvp.Key) && !_systemVariables.Contains(kvp.Key))
                            result.TryAdd(kvp.Key, kvp.Value);
                    }
                }
            }

            CleanClientVariableData(connectionId, moduleId);

            return result;
        }

        public async Task<ConcurrentDictionary<string, object>> UpdateModuleDataAsync(string connectionId, Guid moduleId, Dictionary<string, object> incomingData, string basePath)
        {
            var moduleData = await GetOrCreateModuleDataAsync(connectionId, moduleId, basePath);
            var variables = await _moduleVariableService.GetVariablesAsync(moduleId, ModuleVariableScope.Global, ModuleVariableScope.ServerSide);

            foreach (var variable in variables.Where(v => moduleData.Keys.Contains(v.VariableName) && incomingData.Keys.Contains(v.VariableName)))
            {
                var value = incomingData[variable.VariableName];
                if (value != null && variable.VariableType != "string" && string.IsNullOrEmpty(value.ToString()))
                    value = null;

                Type type = null;

                if (variable.VariableType == "AppModel" && value != null && value.GetType() == typeof(JObject))
                {
                    type = _typeLoaderFactory.GetTypeFromAssembly(variable.ModelTypeRelativePath, variable.ModelTypeFullName);
                    var json = incomingData[variable.VariableName].ToString();
                    value = Newtonsoft.Json.JsonConvert.DeserializeObject(json, type);
                }
                else if (variable.VariableType == "AppModelList" && value != null && value.GetType() == typeof(JArray))
                {
                    type = _typeLoaderFactory.GetTypeFromAssembly(variable.ModelTypeRelativePath, variable.ModelTypeFullName);
                    var listType = typeof(List<>).MakeGenericType(type);
                    var json = incomingData[variable.VariableName].ToString();
                    value = Newtonsoft.Json.JsonConvert.DeserializeObject(json, listType);
                }
                else if (variable.VariableType != "AppModel" && variable.VariableType != "AppModelList" && value != null)
                {
                    type = TypeHelper.ResolveType(variable.VariableType);
                    value = TypeHelper.ConvertValue(value, type);
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
            var cacheKey = "BE_Modules_Variables_App_IsClearCache" + moduleId;
            var checkVariableCaching = _cacheService.Get<bool>(cacheKey);
            if (!checkVariableCaching || !_moduleTemplateCache.TryGetValue(moduleId, out var originalModuleData))
            {
                _serverVariables.TryAdd(moduleId, new ConcurrentBag<string>());
                _clientVariables.TryAdd(moduleId, new ConcurrentBag<string>());

                var moduleData = new ConcurrentDictionary<string, object>();

                var variables = await _moduleVariableService.GetVariablesAsync(moduleId, ModuleVariableScope.Global, ModuleVariableScope.ServerSide);
                foreach (var variable in variables)
                {
                    if (variable.VariableType == "AppModel")
                    {
                        var type = _typeLoaderFactory.GetTypeFromAssembly(variable.ModelTypeRelativePath, variable.ModelTypeFullName);
                        var instance = Activator.CreateInstance(type);

                        moduleData[variable.VariableName] = instance;
                    }
                    else if (variable.VariableType == "AppModelList")
                    {
                        var type = _typeLoaderFactory.GetTypeFromAssembly(variable.ModelTypeRelativePath, variable.ModelTypeFullName);
                        var listType = typeof(List<>).MakeGenericType(type);
                        var emptyList = Activator.CreateInstance(listType);

                        moduleData[variable.VariableName] = emptyList;
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(variable.DefaultValue))
                        {
                            var type = TypeHelper.ResolveType(variable.VariableType);
                            if (type == null)
                            {
                                moduleData[variable.VariableName] = null;
                            }
                            else
                            {
                                var value = TypeHelper.ConvertValue(variable.DefaultValue, type);
                                moduleData[variable.VariableName] = value;
                            }
                        }
                        else
                        {
                            moduleData[variable.VariableName] = null;
                        }
                    }

                    if (variable.Scope == ModuleVariableScope.ClientSide)
                        _clientVariables[moduleId].Add(variable.VariableName);
                    else if (variable.Scope == ModuleVariableScope.ServerSide)
                        _serverVariables[moduleId].Add(variable.VariableName);
                }

                if (_moduleTemplateCache.TryGetValue(moduleId, out var oldData))
                    _moduleTemplateCache.TryUpdate(moduleId, moduleData, oldData);
                else
                    _moduleTemplateCache.TryAdd(moduleId, moduleData);

                originalModuleData = moduleData;

                _cacheService.Set<bool>(cacheKey, true);
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
                    _clientVariables.TryGetValue(moduleId, out var clientVariables);
                    foreach (var key in clientVariables)
                    {
                        if (moduleData.TryGetValue(key, out var data))
                            moduleData.TryUpdate(key, null, data);
                    }
                }
            }
        }
    }
}
