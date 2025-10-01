using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Caching;
using NitroSystem.Dnn.BusinessEngine.Core.Security;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Views;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModuleEngine.Enums;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModuleEngine.Models;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModuleEngine.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModuleEngine.Dto;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Dto;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Enums;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ListItems;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Module;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Services
{
    public class ModuleService : IModuleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly IRepositoryBase _repository;
        private readonly IBuildModule _buildModule;

        public ModuleService(IUnitOfWork unitOfWork, ICacheService cacheService, IRepositoryBase repository, IBuildModule buildModule)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _repository = repository;
            _buildModule = buildModule;
        }

        #region Module Services

        public async Task<ModuleViewModel> GetModuleViewModelAsync(Guid moduleId)
        {
            var module = await _repository.GetAsync<ModuleView>(moduleId);

            return HybridMapper.Map<ModuleView, ModuleViewModel>(module);
        }

        public async Task<IEnumerable<ModuleViewModel>> GetModulesViewModelAsync(Guid scenarioId, PortalSettings portalSettings)
        {
            var modules = await _repository.GetByScopeAsync<ModuleView>(scenarioId);

            return HybridMapper.MapCollection<ModuleView, ModuleViewModel>(modules);
        }

        public async Task<Guid> SaveModuleAsync(ModuleViewModel module, bool isNew)
        {
            var objModuleInfo = HybridMapper.Map<ModuleViewModel, ModuleInfo>(module);

            if (isNew)
                objModuleInfo.Id = await _repository.AddAsync(objModuleInfo);
            else
            {
                var isUpdated = await _repository.UpdateAsync(objModuleInfo);
                if (!isUpdated) ErrorHandling.ThrowUpdateFailedException(objModuleInfo);
            }

            return objModuleInfo.Id;
        }

        public async Task<bool?> IsValidModuleName(Guid scenarioId, Guid? moduleId, string moduleName)
        {
            return await _repository.ExecuteStoredProcedureScalerAsync<bool?>(
                "dbo.BusinessEngine_Studio_IsValidModuleName", "BE_Modules_IsValidModuleName_" + moduleName,
                new
                {
                    ScenarioId = scenarioId,
                    ModuleId = moduleId,
                    ModuleName = moduleName
                });
        }

        public async Task<bool> DeleteModuleAsync(Guid moduleId)
        {
            return await _repository.DeleteAsync<ModuleInfo>(moduleId);
        }

        #endregion

        #region Build Module Services

        public async Task BuildModuleAsync(Guid moduleId, PortalSettings portalSettings, HttpContext context)
        {
            var module = await _repository.GetAsync<ModuleView>(moduleId);

            var spPageIdTask = _repository.ExecuteStoredProcedureScalerAsync<int>(
                "dbo.BusinessEngine_Studio_GetTabIdByDnnModuleId", "BE_TabIdByDnnModuleId" + module.DnnModuleId,
                new
                {
                    module.DnnModuleId
                }
             );

            var spFieldsTask = _repository.ExecuteStoredProcedureMultipleAsync<ModuleFieldInfo, ModuleFieldSettingView>(
                "dbo.BusinessEngine_Studio_GetModuleFieldsAndSettingsForBuild", "BE_Modules_FieldsAndSettingsForBuild" + moduleId,
                new
                {
                    ModuleId = module.Id
                }
            );

            var spResourceReaderTask = _repository.ExecuteStoredProcedureAsDataReaderAsync(
                "dbo.BusinessEngine_Studio_GetModuleResourcesForBuild", "BE_Modules_ResourcesForBuild_" + moduleId,
            new
            {
                ModuleId = module.Id
            });

            await Task.WhenAll(spPageIdTask, spFieldsTask, spResourceReaderTask);

            var pageId = await spPageIdTask;
            var fieldsData = await spFieldsTask;
            var fields = fieldsData.Item1;
            var fieldsSettings = fieldsData.Item2;

            var moduleToBuild = HybridMapper.Map<ModuleView, BuildModuleDto>(module);
            var fieldsToBuild = HybridMapper.MapCollection<ModuleFieldInfo, BuildModuleFieldDto>(fields,
                (src, dest) =>
                {
                    var dict = fieldsSettings.GroupBy(c => c.FieldId)
                                     .ToDictionary(g => g.Key, g => g.AsEnumerable());

                    var items = dict.TryGetValue(src.Id, out var settings);
                    dest.Settings = settings.ToDictionary(x => x.SettingName, x => CastingHelper.ConvertStringToObject(x.SettingValue));

                    dest.GlobalSettings = ReflectionUtil.ConvertDictionaryToObject<ModuleFieldGlobalSettings>(dest.Settings);
                });

            var resourcesToBuild = new List<BuildModuleResourceDto>();
            using (var reader = await spResourceReaderTask)
            {
                while (reader.Read())
                {
                    resourcesToBuild.Add(new BuildModuleResourceDto()
                    {
                        ModuleId = GlobalHelper.GetSafeGuid(reader["ModuleId"]),
                        ResourceType = (ResourceType)reader["ResourceType"],
                        ActionType = (ActionType)reader["ActionType"],
                        ResourcePath = GlobalHelper.GetSafeString(reader["ResourcePath"]),
                        EntryType = GlobalHelper.GetSafeString(reader["EntryType"]),
                        Additional = GlobalHelper.GetSafeString(reader["Additional"]),
                        CacheKey = GlobalHelper.GetSafeString(reader["CacheKey"]),
                        Condition = GlobalHelper.GetSafeString(reader["Condition"]),
                        LoadOrder = GlobalHelper.GetSafeInt(reader["LoadOrder"])
                    });
                }
            }

            var status = await _buildModule.PrepareBuild(moduleToBuild, _repository, portalSettings);
            if (status.IsReadyToBuild)
            {
                var pageResources = await _repository.GetItemsByColumnAsync<PageResourceInfo>("DnnPageId", pageId);
                var pageResourcesDto = HybridMapper.MapCollection<PageResourceInfo, PageResourceDto>(pageResources);

                var finalResources = await _buildModule.ExecuteBuildAsync(moduleToBuild, fieldsToBuild, resourcesToBuild, pageId, portalSettings, context);
                var mappedResources = HybridMapper.MapCollection<PageResourceDto, PageResourceInfo>(finalResources);
                await _repository.BulkInsertAsync<PageResourceInfo>(mappedResources);

                _cacheService.ClearByPrefix("BE_Modules_");
            }
            else if (status.ExceptionError != null)
                throw status.ExceptionError ?? throw new Exception("The module is not ready to be build!");
        }

        #endregion

        #region Module Template Services

        public async Task<ModuleTemplateDto> GetModuleTemplateDtoAsync(Guid moduleId)
        {
            var module = await _repository.GetAsync<ModuleInfo>(moduleId);

            return HybridMapper.Map<ModuleInfo, ModuleTemplateDto>(module);
        }

        public async Task<bool> UpdateModuleTemplateAsync(ModuleTemplateDto module)
        {
            var objModuleInfo = HybridMapper.Map<ModuleTemplateDto, ModuleInfo>(module);

            return await _repository.UpdateAsync(
                objModuleInfo,
                "PreloadingTemplate",
                "LayoutTemplate",
                "LayoutCss"
            );
        }

        #endregion

        #region Module Field Type Services

        public async Task<IEnumerable<ModuleFieldTypeViewModel>> GetFieldTypesViewModelAsync()
        {
            var task1 = _repository.GetAllAsync<ModuleFieldTypeView>("ViewOrder");
            var task2 = _repository.GetAllAsync<ModuleFieldTypeTemplateInfo>("ViewOrder");
            var task3 = _repository.GetAllAsync<ModuleFieldTypeThemeInfo>("ViewOrder");

            await Task.WhenAll(task1, task2, task3);

            var fieldsTypes = await task1;
            var templates = await task2;
            var themes = await task3;

            var builder = new CollectionMappingBuilder<ModuleFieldTypeView, ModuleFieldTypeViewModel>();

            builder.AddChildAsync<ModuleFieldTypeTemplateInfo, ModuleFieldTypeTemplateViewModel, string>(
               source: templates,
               parentKey: parent => parent.FieldType,
               childKey: child => child.FieldType,
               assign: (dest, children) => dest.Templates = children
            );

            builder.AddChildAsync<ModuleFieldTypeThemeInfo, ModuleFieldTypeThemeViewModel, string>(
               source: themes,
               parentKey: parent => parent.FieldType,
               childKey: child => child.FieldType,
               assign: (dest, children) => dest.Themes = children
            );

            var result = await builder.BuildAsync(fieldsTypes);
            return result;
        }

        public async Task<IEnumerable<ModuleFieldTypeCustomEventListItem>> GetFieldTypesCustomEventsListItemAsync(string fieldType)
        {
            var customEvents = await _repository.GetColumnValueAsync<ModuleFieldTypeInfo, string>("CustomEvents", "FieldType", fieldType);

            return !string.IsNullOrEmpty(customEvents)
                ? ReflectionUtil.TryJsonCasting<IEnumerable<ModuleFieldTypeCustomEventListItem>>(customEvents)
                : Enumerable.Empty<ModuleFieldTypeCustomEventListItem>();
        }

        public async Task<string> GetFieldTypeIconAsync(string fieldType)
        {
            return await _repository.GetColumnValueAsync<ModuleFieldTypeInfo, string>("Icon", "FieldType", fieldType);
        }

        #endregion

        #region Module Field Services

        public async Task<IEnumerable<ModuleFieldViewModel>> GetFieldsViewModelAsync(Guid moduleId, string sortBy = "ViewOrder")
        {
            var task1 = _repository.GetByScopeAsync<ModuleFieldInfo>(moduleId, sortBy);
            var task2 = _repository.GetItemsByColumnAsync<ModuleFieldSettingView>("ModuleId", moduleId);
            var task3 = _repository.GetByScopeAsync<ActionInfo>(moduleId, "ParentId", "ViewOrder");

            await Task.WhenAll(task1, task2, task3);

            var fields = await task1;
            var fieldsSettings = await task2;
            var actions = await task3;

            return HybridMapper.MapWithChildren<ModuleFieldInfo, ModuleFieldViewModel, ActionInfo, ActionListItem>(
                parents: fields,
                children: actions,
                parentKeySelector: p => p.Id,
                childKeySelector: c => c.FieldId,
                assignChildren: (parent, childs) => parent.Actions = childs,
                moreAssigns: (src, dest) =>
                {
                    var dict = fieldsSettings.GroupBy(c => c.FieldId).ToDictionary(g => g.Key, g => g.AsEnumerable());
                    var items = dict.TryGetValue(src.Id, out var settings);
                    dest.Settings = settings.ToDictionary(x => x.SettingName, x => CastingHelper.ConvertStringToObject(x.SettingValue));

                }
            );
        }

        public async Task<ModuleFieldViewModel> GetFieldViewModelAsync(Guid fieldId)
        {
            var task1 = _repository.GetAsync<ModuleFieldInfo>(fieldId);
            var task2 = _repository.GetItemsByColumnAsync<ModuleFieldSettingView>("FieldId", fieldId);
            var task3 = _repository.GetItemsByColumnAsync<ActionInfo>("FieldId", fieldId);

            await Task.WhenAll(task1, task2, task3);

            var field = await task1;
            var fieldSettings = await task2;
            var actions = await task3;

            return HybridMapper.MapWithChildren<ModuleFieldInfo, ModuleFieldViewModel, ActionInfo, ActionListItem>(
                  source: field,
                  children: actions,
                  assignChildren: (parent, childs) => parent.Actions = childs,
                  moreAssigns: (src, dest) =>
                  {
                      dest.Settings = fieldSettings.ToDictionary(x => x.SettingName, x => CastingHelper.ConvertStringToObject(x.SettingValue));
                  }
               );
        }

        public async Task<Guid> SaveFieldAsync(ModuleFieldViewModel field, bool isNew)
        {
            var objModuleFieldInfo = HybridMapper.Map<ModuleFieldViewModel, ModuleFieldInfo>(field);

            _unitOfWork.BeginTransaction();

            try
            {
                if (isNew)
                {
                    objModuleFieldInfo.Id = await _repository.AddAsync<ModuleFieldInfo>(objModuleFieldInfo, true);
                }
                else
                {
                    var isUpdated = await _repository.UpdateAsync<ModuleFieldInfo>(objModuleFieldInfo);
                    if (!isUpdated) ErrorHandling.ThrowUpdateFailedException(objModuleFieldInfo);

                    await _repository.DeleteByScopeAsync<ModuleFieldSettingInfo>(field.Id);
                }

                if (field.Settings != null)
                {
                    foreach (var setting in field.Settings)
                    {
                        var value = setting.Value != null && setting.Value.GetType().IsClass && !(setting.Value is string)
                            ? JsonConvert.SerializeObject(setting.Value)
                            : setting.Value?.ToString();

                        var objModuleFieldSettingInfo = new ModuleFieldSettingInfo()
                        {
                            FieldId = objModuleFieldInfo.Id,
                            SettingName = setting.Key,
                            SettingValue = value
                        };

                        await _repository.AddAsync<ModuleFieldSettingInfo>(objModuleFieldSettingInfo);
                    }
                }

                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();

                throw ex;
            }

            return objModuleFieldInfo.Id;
        }

        public async Task<bool> UpdateFieldPaneAsync(SortPaneFieldsDto data)
        {
            return await _repository.UpdateColumnAsync<ModuleFieldInfo>("PaneName", data.PaneName, data.FieldId);
        }

        public async Task SortFieldsAsync(SortPaneFieldsDto data)
        {
            await _repository.ExecuteStoredProcedureAsync("dbo.BusinessEngine_Studio_SortModuleFields",
                new
                {
                    ModuleId = data.ModuleId,
                    PaneName = data.PaneName,
                    FieldIds = JsonConvert.SerializeObject(data.PaneFieldIds)
                }
            );

            var cacheKey = AttributeCache.Instance.GetCache<ModuleFieldInfo>().key;
            _cacheService.RemoveByPrefix(cacheKey);
        }

        public async Task<bool> DeleteFieldAsync(Guid moduleId)
        {
            _unitOfWork.BeginTransaction();

            try
            {
                var task1 = _repository.DeleteByScopeAsync<ModuleFieldSettingInfo>(moduleId);
                var task2 = _repository.DeleteAsync<ModuleFieldInfo>(moduleId);

                await Task.WhenAll(task1, task2);

                _unitOfWork.Commit();

                return true;
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();

                throw ex;
            }
        }

        #endregion

        #region Module Variable Services

        public async Task<IEnumerable<ModuleVariableViewModel>> GetModuleVariablesViewModelAsync(Guid moduleId)
        {
            var variables = await _repository.GetByScopeAsync<ModuleVariableInfo>(moduleId, "ViewOrder");

            return HybridMapper.MapCollection<ModuleVariableInfo, ModuleVariableViewModel>(variables);
        }

        public async Task<IEnumerable<ModuleVariableDto>> GetModuleVariablesDtoAsync(Guid moduleId)
        {
            var variables = await _repository.GetByScopeAsync<ModuleVariableInfo>(moduleId, "VariableName");
            var properties = await _repository.ExecuteStoredProcedureAsListAsync<AppModelPropertyInfo>(
                "dbo.BusinessEngine_Studio_GetAppModelPropertiesAsModuleVariables", "BE_AppModelProperties_ModuleVariables_" + moduleId,
                new
                {
                    ModuleId = moduleId
                });

            return HybridMapper.MapWithChildren<ModuleVariableInfo, ModuleVariableDto, AppModelPropertyInfo, PropertyInfo>(
                parents: variables,
                children: properties,
                parentKeySelector: p => p.AppModelId,
                childKeySelector: c => c.AppModelId,
                assignChildren: (parent, childs) => parent.Properties = childs
            );
        }

        public async Task<Guid> SaveModuleVariablesAsync(ModuleVariableViewModel variable, bool isNew)
        {
            var objModuleVariableInfo = HybridMapper.Map<ModuleVariableViewModel, ModuleVariableInfo>(variable);

            if (isNew)
            {
                objModuleVariableInfo.Id = await _repository.AddAsync<ModuleVariableInfo>(objModuleVariableInfo, true);
            }
            else
            {
                var isUpdated = await _repository.UpdateAsync<ModuleVariableInfo>(objModuleVariableInfo);
                if (!isUpdated) ErrorHandling.ThrowUpdateFailedException(objModuleVariableInfo);
            }

            return objModuleVariableInfo.Id;
        }

        public async Task<bool> DeleteModuleVariablesAsync(Guid moduleId)
        {
            return await _repository.DeleteAsync<ModuleVariableInfo>(moduleId);
        }

        #endregion

        #region Module Libraries & Resources

        public async Task<IEnumerable<ModuleCustomLibraryViewModel>> GetModuleCustomLibrariesAsync(Guid moduleId)
        {
            var libraries = await _repository.GetByScopeAsync<ModuleCustomLibraryView>(moduleId, "LoadOrder");
            var resources = await _repository.GetByScopeAsync<ModuleCustomLibraryResourceView>(moduleId);

            return HybridMapper.MapWithChildren<ModuleCustomLibraryView, ModuleCustomLibraryViewModel,
                                                    ModuleCustomLibraryResourceView, ModuleCustomLibraryResourceViewModel>(
                parents: libraries,
                children: resources,
                parentKeySelector: p => p.Id,
                childKeySelector: c => c.LibraryId,
                assignChildren: (parent, childs) => parent.Resources = childs
            );
        }

        public async Task<IEnumerable<ModuleCustomResourceViewModel>> GetModuleCustomResourcesAsync(Guid moduleId)
        {
            var resources = await _repository.GetByScopeAsync<ModuleCustomResourceInfo>(moduleId, "LoadOrder");

            return HybridMapper.MapCollection<ModuleCustomResourceInfo, ModuleCustomResourceViewModel>(resources);
        }

        public async Task<Guid> SaveModuleCustomLibraryAsync(ModuleCustomLibraryViewModel library, bool isNew)
        {
            var objModuleCustomLibraryInfo = HybridMapper.Map<ModuleCustomLibraryViewModel, ModuleCustomLibraryInfo>(library);

            if (isNew)
                objModuleCustomLibraryInfo.Id = await _repository.AddAsync<ModuleCustomLibraryInfo>(objModuleCustomLibraryInfo);
            else
            {
                var isUpdated = await _repository.UpdateAsync<ModuleCustomLibraryInfo>(objModuleCustomLibraryInfo);
                if (!isUpdated) ErrorHandling.ThrowUpdateFailedException(objModuleCustomLibraryInfo);
            }

            return objModuleCustomLibraryInfo.Id;
        }

        public async Task<Guid> SaveModuleCustomResourceAsync(ModuleCustomResourceViewModel resource, bool isNew)
        {
            var objModuleCustomResourceInfo = HybridMapper.Map<ModuleCustomResourceViewModel, ModuleCustomResourceInfo>(resource);

            if (isNew)
                objModuleCustomResourceInfo.Id = await _repository.AddAsync<ModuleCustomResourceInfo>(objModuleCustomResourceInfo);
            else
            {
                var isUpdated = await _repository.UpdateAsync<ModuleCustomResourceInfo>(objModuleCustomResourceInfo);
                if (!isUpdated) ErrorHandling.ThrowUpdateFailedException(objModuleCustomResourceInfo);
            }

            return objModuleCustomResourceInfo.Id;
        }

        public async Task SortModuleCustomLibraries(LibraryOrResource target, IEnumerable<SortInfo> items)
        {
            if (target == LibraryOrResource.Library)
                await _repository.ExecuteStoredProcedureAsync("dbo.BusinessEngine_Studio_SortModuleCustomLibraries",
                    new { JsonData = items.ToJson() });
            else if (target == LibraryOrResource.Resource)
                await _repository.ExecuteStoredProcedureAsync("dbo.BusinessEngine_Studio_SortModuleCustomResources",
                    new { JsonData = items.ToJson() });

            DataCache.ClearCache("BE_ModuleCustomLibraries_");
            DataCache.ClearCache("BE_ModuleCustomResources_");
        }

        public async Task<bool> DeleteModuleCustomLibraryAsync(Guid moduleId)
        {
            return await _repository.DeleteAsync<ModuleCustomLibraryInfo>(moduleId);
        }

        public async Task<bool> DeleteModuleCustomResourceAsync(Guid moduleId)
        {
            return await _repository.DeleteAsync<ModuleCustomResourceInfo>(moduleId);
        }

        #endregion
    }
}
