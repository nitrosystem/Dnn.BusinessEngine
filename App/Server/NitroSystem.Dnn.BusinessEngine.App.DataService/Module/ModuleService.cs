using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Views;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Data.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Web.Dto;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Procedures;

namespace NitroSystem.Dnn.BusinessEngine.App.DataService.Module
{
    public class ModuleService : IModuleService
    {
        private readonly IRepositoryBase _repository;

        public ModuleService(IRepositoryBase repository)
        {
            _repository = repository;
        }

        #region Module Services

        public async Task<ModuleDto> GetModuleViewModelAsync(Guid moduleId)
        {
            var module = await _repository.GetAsync<ModuleView>(moduleId);

            return HybridMapper.Map<ModuleView, ModuleDto>(module);
        }

        public ModuleLiteDto GetModuleLiteData(int? siteModuleId, Guid? moduleId = null)
        {
            var module = _repository.ExecuteStoredProcedure<ModuleLiteResult>(
                "dbo.BusinessEngine_App_GetModuleLite", "Be_Modules_ModuleLiteData",
                new
                {
                    SiteModuleId = siteModuleId,
                    ModuleId = moduleId
                });

            return HybridMapper.Map<ModuleLiteResult, ModuleLiteDto>(module);
        }

        public async Task<string> GetModuleNameAsync(Guid moduleId)
        {
            return await _repository.GetColumnValueAsync<ModuleInfo, string>(moduleId, "ModuleName");
        }

        public async Task<string> GetScenarioNameAsync(Guid moduleId)
        {
            return await _repository.GetColumnValueAsync<ModuleView, string>(moduleId, "ScenarioName");
        }

        #endregion

        #region Module Field Services

        public async Task<IEnumerable<ModuleFieldDto>> GetFieldsDtoAsync(Guid moduleId)
        {
            var fields = await _repository.GetByScopeAsync<ModuleFieldInfo>(moduleId, "ViewOrder");
            var fieldsSettings = await _repository.GetItemsByColumnAsync<ModuleFieldSettingView>("ModuleId", moduleId);

            return await HybridMapper.MapCollectionAsync<ModuleFieldInfo, ModuleFieldDto>(fields,
                async (src, dest) =>
                {
                    dest.DataSource = src.HasDataSource && !string.IsNullOrWhiteSpace(src.DataSource)
                               ? await GetFieldDataSource(src.DataSource)
                               : null;

                    var dict = fieldsSettings.GroupBy(c => c.FieldId).ToDictionary(g => g.Key, g => g.AsEnumerable());
                    if (dict.TryGetValue(src.Id, out var settings))
                        dest.Settings = settings.ToDictionary(x => x.SettingName, x => CastingHelper.ConvertStringToObject(x.SettingValue));
                    else
                        dest.Settings = new Dictionary<string, object>();
                }
            );
        }

        public async Task<ModuleFieldDto> GetFieldDtoAsync(Guid fieldId, bool includeDataSource = false)
        {
            var field = await _repository.GetAsync<ModuleFieldInfo>(fieldId);
            var fieldSettings = await _repository.GetByScopeAsync<ModuleFieldSettingInfo>(fieldId);

            return await HybridMapper.MapAsync<ModuleFieldInfo, ModuleFieldDto>(field,
                async (src, dest) =>
                {
                    if (includeDataSource)
                        dest.DataSource = src.HasDataSource && !string.IsNullOrWhiteSpace(src.DataSource)
                               ? await GetFieldDataSource(src.DataSource)
                               : null;

                    dest.Settings = fieldSettings.ToDictionary(x => x.SettingName, x => CastingHelper.ConvertStringToObject(x.SettingValue));
                }
            );
        }

        public async Task<ModuleFieldDataSourceResult> GetFieldDataSource(string dataSourceSettings)
        {
            var dataSource = JsonConvert.DeserializeObject<ModuleFieldDataSourceInfo>(dataSourceSettings);

            ModuleFieldDataSourceResult result = HybridMapper.Map<ModuleFieldDataSourceInfo, ModuleFieldDataSourceResult>(dataSource);

            if ((dataSource.Type == ModuleFieldDataSourceType.FieldOptions || dataSource.Type == ModuleFieldDataSourceType.DefinedList) &&
                !string.IsNullOrEmpty(dataSource.ListName))
            {
                result.Items = await _repository.GetByScopeAsync<DefinedListItemView>(dataSource.ListName); ;
            }

            return result;
        }

        #endregion

        #region Module Variables

        public async Task<IEnumerable<ModuleVariableDto>> GetVariables(Guid moduleId, ModuleVariableScope scope)
        {
            var results = await _repository.ExecuteStoredProcedureMultipleAsync<ModuleVariableView, AppModelPropertyInfo>(
                "dbo.BusinessEngine_App_GetModuleVariables", "BE_ModuleVariables_App_",
                    new
                    {
                        ModuleId = moduleId,
                        Scope = scope,
                    }
                );

            var variables = results.Item1;
            var properties = results.Item2;

            return HybridMapper.MapWithChildren<ModuleVariableView, ModuleVariableDto, AppModelPropertyInfo, PropertyInfo>(
               parents: variables,
               children: properties,
               parentKeySelector: p => p.AppModelId,
               childKeySelector: c => c.AppModelId,
               assignChildren: (parent, childs) => parent.Properties = childs
           );
        }

        public async Task<IEnumerable<ModuleClientVariableDto>> GetClientVariables(Guid moduleId)
        {
            var variables = await _repository.GetByScopeAsync<ModuleVariableInfo>(moduleId);

            return HybridMapper.MapCollection<ModuleVariableInfo, ModuleClientVariableDto>(variables);
        }

        #endregion
    }
}
