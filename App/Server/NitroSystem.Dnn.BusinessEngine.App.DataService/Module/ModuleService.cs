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
using NitroSystem.Dnn.BusinessEngine.Abstractions.ModuleBuilder.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ModuleBuilder.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Contracts;

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

        #endregion

        #region Module Field Services

        public async Task<IEnumerable<ModuleFieldDto>> GetFieldsViewModelAsync(Guid moduleId)
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
                    var items = dict.TryGetValue(src.Id, out var settings);
                    dest.Settings = settings.ToDictionary(x => x.SettingName, x => CastingHelper.ConvertStringToObject(x.SettingValue));
                }
            );
        }

        public async Task<FieldDataSourceResult> GetFieldDataSource(string dataSourceSettings)
        {
            var dataSource = JsonConvert.DeserializeObject<FieldDataSourceInfo>(dataSourceSettings);

            FieldDataSourceResult result = HybridMapper.Map<FieldDataSourceInfo, FieldDataSourceResult>(dataSource);

            if ((dataSource.Type == FieldDataSourceType.StaticItems || dataSource.Type == FieldDataSourceType.UseDefinedList) &&
                dataSource.ListId != null)
            {
                result.Items = await _repository.GetByScopeAsync<DefinedListItemView>(dataSource.ListId); ;
            }

            return result;
        }

        #endregion

        #region Module Variables

        public async Task<IEnumerable<ModuleVariableDto>> GetModuleVariables(Guid moduleId, ModuleVariableScope scope)
        {
            var results = await _repository.ExecuteStoredProcedureMultipleAsync<ModuleVariableView, AppModelPropertyInfo>(
                "BusinessEngine_App_GetModuleVariables", "BE_ModuleVariables_App_" + moduleId,
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

        public async Task<IEnumerable<ModuleClientVariableDto>> GetModuleClientVariables(Guid moduleId)
        {
            var variables = await _repository.GetByScopeAsync<ModuleVariableInfo>(moduleId);

            return HybridMapper.MapCollection<ModuleVariableInfo, ModuleClientVariableDto>(variables);
        }

        #endregion
    }
}
