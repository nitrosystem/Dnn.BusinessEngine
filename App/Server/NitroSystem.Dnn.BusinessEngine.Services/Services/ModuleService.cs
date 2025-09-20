using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Tokens;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Common.IO;
using NitroSystem.Dnn.BusinessEngine.Common.Reflection;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Cashing;
using NitroSystem.Dnn.BusinessEngine.Core.Security;
using NitroSystem.Dnn.BusinessEngine.Core.Mapper;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;
using NitroSystem.Dnn.BusinessEngine.App.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.App.Services.Dto;
using NitroSystem.Dnn.BusinessEngine.App.Services.Enums;
using NitroSystem.Dnn.BusinessEngine.App.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.App.Services.ViewModels.Module.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using NitroSystem.Dnn.BusinessEngine.App.Services.ViewModels.Module;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using NitroSystem.Dnn.BusinessEngine.Utilities;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using DotNetNuke.Services.Scheduling;
using NitroSystem.Dnn.BusinessEngine.Common.Models.Shared;
using NitroSystem.Dnn.BusinessEngine.Common.Enums;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Views;

namespace NitroSystem.Dnn.BusinessEngine.App.Services.Services
{
    public class ModuleService : IModuleService
    {
        private readonly IRepositoryBase _repository;

        public ModuleService(IRepositoryBase repository)
        {
            _repository = repository;
        }

        #region Module Services

        public ModuleViewModel GetModuleViewModel(Guid moduleId)
        {
            var module = _repository.Get<ModuleView>(moduleId);

            return HybridMapper.MapWithConfig<ModuleView, ModuleViewModel>(
               module, (src, dest) =>
               {
                   dest.Settings = string.IsNullOrEmpty(module.Settings)
                   ? null
                   : TypeCasting.TryJsonCasting<IDictionary<string, object>>(module.Settings.ToString());
               });
        }

        public async Task<ModuleViewModel> GetModuleViewModelAsync(Guid moduleId)
        {
            var module = await _repository.GetAsync<ModuleView>(moduleId);

            return HybridMapper.MapWithConfig<ModuleView, ModuleViewModel>(
               module, (src, dest) =>
               {
                   dest.Settings = string.IsNullOrEmpty(module.Settings)
                   ? null
                   : TypeCasting.TryJsonCasting<IDictionary<string, object>>(module.Settings.ToString());
               });
        }

        #endregion

        #region Module Field Services

        public async Task<IEnumerable<ModuleFieldViewModel>> GetFieldsViewModelAsync(Guid moduleId)
        {
            var task1 = _repository.GetByScopeAsync<ModuleFieldInfo>(moduleId, "ViewOrder");
            var task2 = _repository.GetItemsByColumnAsync<ModuleFieldSettingView>("ModuleId", moduleId);

            var fields = await task1;
            var settings = await task2;

            return await Task.WhenAll(
                fields.Select(async field =>
                {
                    return await HybridMapper.MapWithConfigAsync<ModuleFieldInfo, ModuleFieldViewModel>(field,
                        async (src, dest) =>
                        {
                            dest.ConditionalValues = TypeCasting.TryJsonCasting<IEnumerable<FieldValueInfo>>(field.ConditionalValues);
                            dest.DataSource = field.HasDataSource && !string.IsNullOrWhiteSpace(field.DataSource)
                                ? await GetFieldDataSource(field.DataSource)
                                : null;
                            dest.Settings = settings != null && settings.Any()
                                ? settings
                                    .Where(x => x.FieldId == field.Id)
                                    .ToDictionary(x => x.SettingName, x => Globals.ConvertStringToObject(x.SettingValue))
                                : null;
                        });
                })
            );
        }

        public IEnumerable<ModuleFieldViewModel> GetFieldsViewModel(Guid moduleId)
        {
            var fields = _repository.GetByScope<ModuleFieldInfo>(moduleId, "ViewOrder");
            var settings = _repository.GetItemsByColumn<ModuleFieldSettingView>("ModuleId", moduleId);

            return fields.Select(field =>
                {
                    return HybridMapper.MapWithConfig<ModuleFieldInfo, ModuleFieldViewModel>(field,
                        async (src, dest) =>
                        {
                            dest.ConditionalValues = TypeCasting.TryJsonCasting<IEnumerable<FieldValueInfo>>(field.ConditionalValues);
                            dest.DataSource = field.HasDataSource && !string.IsNullOrWhiteSpace(field.DataSource)
                                ? await GetFieldDataSource(field.DataSource)
                                : null;
                            dest.Settings = settings != null && settings.Any()
                                ? settings
                                    .Where(x => x.FieldId == field.Id)
                                    .ToDictionary(x => x.SettingName, x => Globals.ConvertStringToObject(x.SettingValue))
                                : null;
                        });
                });
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

        public async Task<IEnumerable<ModuleVariableDto>> GetModuleVariablesAsync(Guid moduleId, ModuleVariableScope scope)
        {
            var results = await _repository.ExecuteStoredProcedureMultiGridResultAsync(
                "BusinessEngine_App_GetModuleVariables", "App_ModuleVariables_",
                    new
                    {
                        ModuleId = moduleId,
                        Scope = scope,
                    },
                    grid => grid.Read<ModuleVariableView>(),
                    grid => grid.Read<AppModelPropertyInfo>()
                );

            var variables = results[0] as IEnumerable<ModuleVariableView>;
            var appModelsProperties = results[1] as IEnumerable<AppModelPropertyInfo>;

            return variables.Select(variable =>
            {
                return HybridMapper.MapWithConfig<ModuleVariableView, ModuleVariableDto>(variable,
                (src, dest) =>
                {
                    dest.Scope = (ModuleVariableScope)variable.Scope;
                    dest.Properties = appModelsProperties.Where(p => p.AppModelId == variable.AppModelId).Select(prop =>
                    {
                        return HybridMapper.Map<AppModelPropertyInfo, PropertyInfo>(prop);
                    });
                });
            });
        }

        public IEnumerable<ModuleVariableDto> GetModuleVariables(Guid moduleId, ModuleVariableScope scope)
        {
            var results = _repository.ExecuteStoredProcedureMultiGridResult(
                "BusinessEngine_App_GetModuleVariables", "App_ModuleVariables_",
                    new
                    {
                        ModuleId = moduleId,
                        Scope = scope,
                    },
                    grid => grid.Read<ModuleVariableView>(),
                    grid => grid.Read<AppModelPropertyInfo>()
                );

            var variables = results[0] as IEnumerable<ModuleVariableView>;
            var appModelsProperties = results[1] as IEnumerable<AppModelPropertyInfo>;

            return variables.Select(variable =>
            {
                return HybridMapper.MapWithConfig<ModuleVariableView, ModuleVariableDto>(variable,
                (src, dest) =>
                {
                    dest.Scope = (ModuleVariableScope)variable.Scope;
                    dest.Properties = appModelsProperties.Where(p => p.AppModelId == variable.AppModelId).Select(prop =>
                    {
                        return HybridMapper.Map<AppModelPropertyInfo, PropertyInfo>(prop);
                    });
                });
            });
        }

        public async Task<IEnumerable<ModuleClientVariableDto>> GetModuleClientVariables(Guid moduleId)
        {
            var variables = await _repository.GetByScopeAsync<ModuleVariableInfo>(moduleId);

            return variables.Select(variable =>
                HybridMapper.Map<ModuleVariableInfo, ModuleClientVariableDto>(variable)
            );
        }

        #endregion
    }
}
