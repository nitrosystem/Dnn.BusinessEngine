using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Tokens;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Common.IO;
using NitroSystem.Dnn.BusinessEngine.Common.Reflection;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Cashing;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Core.Mapper;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Enums;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.Dto;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Dto;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Enums;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Mapping;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Module.Field;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using NitroSystem.Dnn.BusinessEngine.Utilities;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Models;
using NitroSystem.Dnn.BusinessEngine.Common.TypeCasting;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Services
{
    public class ModuleService : IModuleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly IRepositoryBase _repository;
        private readonly IBuildModuleService _buildModuleService;

        public ModuleService(IUnitOfWork unitOfWork, ICacheService cacheService, IRepositoryBase repository, IBuildModuleService buildModuleService)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _repository = repository;
            _buildModuleService = buildModuleService;
        }

        #region Module Services

        public async Task<ModuleViewModel> GetModuleViewModelAsync(Guid id, PortalSettings portalSettings)
        {
            var module = await _repository.GetAsync<ModuleView>(id);

            return ModuleMapping.MapModuleViewModel(module);
        }

        public async Task<IEnumerable<ModuleViewModel>> GetModulesViewModelAsync(Guid scenarioId, PortalSettings portalSettings)
        {
            var modules = await _repository.GetByScopeAsync<ModuleView>(scenarioId);

            return null;
            //return ModuleMapping.GetModulesViewModel(modules, toupleList);
        }

        public async Task<Guid> SaveModuleAsync(ModuleViewModel module, bool isNew)
        {
            var objModuleInfo = ModuleMapping.MapModuleInfo(module);

            if (isNew)
                objModuleInfo.Id = await _repository.AddAsync<ModuleInfo>(objModuleInfo);
            else
            {
                var isUpdated = await _repository.UpdateAsync<ModuleInfo>(objModuleInfo);
                if (!isUpdated) ErrorService.ThrowUpdateFailedException(objModuleInfo);
            }

            return objModuleInfo.Id;
        }

        public async Task BuildModuleAsync(BuildModuleRequest postData, PortalSettings portalSettings, HttpContext context)
        {
            if (postData.Scope == BuildScope.OnlyOneModule && !postData.ModuleId.HasValue)
                throw new ArgumentNullException(nameof(postData.ModuleId));
            else if (postData.Scope == BuildScope.AllScenarioModules && !postData.ScenarioId.HasValue)
                throw new ArgumentNullException(nameof(postData.ScenarioId));

            var modules = await _repository.ExecuteStoredProcedureAsListAsync<BuildModuleView>("BusinessEngine_GetBuildModulesByScope",
                new
                {
                    postData.Scope,
                    postData.ModuleId,
                    postData.ScenarioId
                });

            foreach (var module in modules)
            {
                var spResourceReaderTask = _repository.ExecuteStoredProcedureAsDataReaderAsync("BusinessEngine_GetBuildModulesAllResources",
                new
                {
                    ModuleId = module.Id
                });

                var spFieldsTask = _repository.ExecuteStoredProcedureMultiGridResultAsync("BusinessEngine_GetBuildModulesFieldsAndSettings",
                    new
                    {
                        ModuleId = module.Id
                    },
                    grid => grid.Read<ModuleFieldInfo>(),
                    grid => grid.Read<ModuleFieldSettingView>()
                );

                var spPageIdTask = _repository.ExecuteStoredProcedureScalerAsync<int>("BusinessEngine_GetTabIdByDnnModuleId",
                    new
                    {
                        module.DnnModuleId
                    }
                 );

                await Task.WhenAll(spResourceReaderTask, spFieldsTask, spPageIdTask);

                var fieldsData = await spFieldsTask;
                var fields = (IEnumerable<ModuleFieldInfo>)fieldsData[0];
                var fieldsSettings = (IEnumerable<ModuleFieldSettingView>)fieldsData[1];
                var pageId = await spPageIdTask;

                var moduleToBuild = new BuildModuleDto();
                module.CopyProperties(moduleToBuild);

                var fieldsToBuild = ModuleMapping.MapModuleFieldsDto(fields, fieldsSettings);

                var resourcesToBuild = new List<BuildModuleResourceDto>();
                using (var reader = await spResourceReaderTask)
                {
                    while (reader.Read())
                    {
                        resourcesToBuild.Add(new BuildModuleResourceDto()
                        {
                            ModuleId = Globals.GetSafeGuid(reader["ModuleId"]),
                            ResourceType = (ResourceType)reader["ResourceType"],
                            ActionType = (ActionType)reader["ActionType"],
                            ResourcePath = Globals.GetSafeString(reader["ResourcePath"]),
                            EntryType = Globals.GetSafeString(reader["EntryType"]),
                            Additional = Globals.GetSafeString(reader["Additional"]),
                            CacheKey = Globals.GetSafeString(reader["CacheKey"]),
                            Condition = Globals.GetSafeString(reader["Condition"]),
                            LoadOrder = Globals.GetSafeInt(reader["LoadOrder"])
                        });
                    }
                }

                var status = await _buildModuleService.PrepareBuild(moduleToBuild, _repository, portalSettings);
                if (status.IsReadyToBuild)
                {
                    var pageResources = await _repository.GetItemsByColumnAsync<PageResourceInfo>("DnnPageId", pageId);
                    var pageResourcesDto = ResourceMapping.MapPageResourcesDto(pageResources);

                    var finalResources = await _buildModuleService.ExecuteBuildAsync(moduleToBuild, fieldsToBuild, resourcesToBuild, pageId, portalSettings, context);
                    var mappedResources = ResourceMapping.MapPageResourcesInfo(finalResources);
                    await _repository.BulkInsertAsync<PageResourceInfo>(mappedResources);
                }
                else if (status.ExceptionError != null)
                    throw status.ExceptionError ?? throw new Exception("The module is not ready to be build!");
            }
        }

        //public async Task<string> GetModuleActionScriptsAsync(IEnumerable<ModuleActionTypeView> actionTypes, HttpContext context)
        //{
        //    var scripts = new StringBuilder();

        //    foreach (var actionType in actionTypes)
        //    {
        //        string cacheKey = $"BE_ModuleActionTypes_Views_{actionType.ActionType}";

        //        scripts.AppendLine("//Start Action Type : " + actionType.ActionType);

        //        var result = _cacheService.Get<string>(cacheKey);
        //        if (string.IsNullOrEmpty(result))
        //        {
        //            string actionJsMapPath = context.Server.MapPath($"~{actionType.ActionJsPath.Replace("[EXTPATH]", "/DesktopModules/BusinessEngine/extensions")}");
        //            result = await FileUtil.GetFileContentAsync(actionJsMapPath);

        //            scripts.AppendLine(result);

        //            _cacheService.Set<string>(cacheKey, result);
        //        }

        //        scripts.AppendLine("//End Action Type : " + actionType.ActionType);
        //    }

        //    return scripts.ToString();
        //}

        public async Task<bool> UpdateModuleLayoutTemplateAsync(ModuleLayoutTemplateDto data)
        {
            var objModuleInfo = new ModuleInfo()
            {
                Id = data.ModuleId,
                PreloadingTemplate = data.PreloadingTemplate,
                LayoutTemplate = data.LayoutTemplate,
                LayoutCss = data.LayoutCss
            };

            return await _repository.UpdateAsync<ModuleInfo>(
                objModuleInfo,
                "PreloadingTemplate",
                "LayoutTemplate",
                "LayoutCss"
            );
        }

        public async Task<bool?> IsValidModuleName(Guid scenarioId, Guid? moduleId, string moduleName)
        {
            return await _repository.ExecuteStoredProcedureScalerAsync<bool?>("BusinessEngine_IsValidModuleName",
                new { ScenarioId = scenarioId, ModuleId = moduleId, ModuleName = moduleName });
        }

        public async Task<bool> DeleteModuleAsync(Guid id)
        {
            return await _repository.DeleteAsync<ModuleInfo>(id);
        }

        #endregion

        #region Module Field Type Services

        public async Task<IEnumerable<ModuleFieldTypeViewModel>> GetFieldTypesViewModelAsync()
        {
            var task1 = _repository.GetAllAsync<ModuleFieldTypeView>("ViewOrder");
            var task2 = _repository.GetAllAsync<ModuleFieldTypeTemplateInfo>("ViewOrder");
            var task3 = _repository.GetAllAsync<ModuleFieldTypeThemeInfo>("ViewOrder");

            await Task.WhenAll(task1, task2, task3);

            return ModuleMapping.MapModuleFieldTypesViewModel(await task1, await task2, await task3);
        }

        public async Task<IEnumerable<ModuleFieldTypeCustomEventListItem>> GetFieldTypesGetCustomEventsAsync(string fieldType)
        {
            var customEvents = await _repository.GetColumnValueAsync<ModuleFieldTypeInfo, string>("CustomEvents", "FieldType", fieldType);

            return !string.IsNullOrEmpty(fieldType)
                ? TypeCastingUtil<IEnumerable<ModuleFieldTypeCustomEventListItem>>.TryJsonCasting(customEvents)
                : Enumerable.Empty<ModuleFieldTypeCustomEventListItem>();
        }

        private async Task<string> GetFieldTypeGeneratePanesBusinessControllerClass(string fieldType)
        {
            return await _repository.GetColumnValueAsync<ModuleFieldTypeInfo, string>("GeneratePanesBusinessControllerClass",
                "FieldType", fieldType);
        }

        private async Task<string> GetFieldTypeScript(string fieldType, HttpContext context)
        {
            string cacheKey = $"BE_ModuleFieldTypes_Script_{fieldType}";

            var result = _cacheService.Get<string>(cacheKey);
            if (string.IsNullOrEmpty(result))
            {
                var scripts = new StringBuilder();
                scripts.AppendLine("//Start Field Type : " + fieldType);

                var objModuleFieldTypeInfo = await _repository.GetByColumnAsync<ModuleFieldTypeInfo>("FieldType", fieldType);

                string fieldJsMapPath = context.Server.MapPath($"~{(objModuleFieldTypeInfo?.FieldJsPath ?? string.Empty).Replace("[EXTPATH]", "/DesktopModules/BusinessEngine/extensions")}");
                scripts.AppendLine(await FileUtil.GetFileContentAsync(fieldJsMapPath));

                scripts.AppendLine("//End Field Type : " + fieldType);

                if (!string.IsNullOrEmpty(objModuleFieldTypeInfo?.DirectiveJsPath))
                {
                    scripts.AppendLine("//Start Directive of Field Type : " + fieldType);

                    string directiveJsMapPath = context.Server.MapPath($"~{(objModuleFieldTypeInfo?.DirectiveJsPath ?? string.Empty).Replace("[EXTPATH]", "/DesktopModules/BusinessEngine/extensions")}");
                    scripts.AppendLine(await FileUtil.GetFileContentAsync(directiveJsMapPath));

                    scripts.AppendLine("//End Directive of Field Type : " + fieldType);
                }

                result = scripts.ToString();

                _cacheService.Set(cacheKey, result);
            }

            return result;
        }

        #endregion

        #region Module Field Services

        public async Task<IEnumerable<ModuleFieldViewModel>> GetFieldsViewModelAsync(Guid moduleId, string sortBy = "ViewOrder")
        {
            var task1 = _repository.GetByScopeAsync<ModuleFieldInfo>(moduleId, sortBy);
            var task2 = _repository.GetItemsByColumnAsync<ModuleFieldSettingView>("ModuleId", moduleId);

            await Task.WhenAll(task1, task2);

            return ModuleMapping.MapModuleFieldsViewModel(await task1, await task2);
        }

        public async Task<ModuleFieldViewModel> GetFieldViewModelAsync(Guid fieldId)
        {
            var task1 = _repository.GetAsync<ModuleFieldInfo>(fieldId);
            var task2 = _repository.GetItemsByColumnAsync<ModuleFieldSettingView>("FieldId", fieldId);

            await Task.WhenAll(task1, task2);

            return ModuleMapping.MapModuleFieldViewModel(await task1, await task2);
        }

        public async Task<Guid> SaveFieldAsync(ModuleFieldViewModel field, bool isNew)
        {
            var objModuleFieldInfo = ModuleMapping.MapModuleFieldInfo(field);

            if (isNew)
            {
                objModuleFieldInfo.Id = await _repository.AddAsync<ModuleFieldInfo>(objModuleFieldInfo, true);
            }
            else
            {
                var isUpdated = await _repository.UpdateAsync<ModuleFieldInfo>(objModuleFieldInfo);
                if (!isUpdated) ErrorService.ThrowUpdateFailedException(objModuleFieldInfo);

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

                    await _repository.AddAsync(objModuleFieldSettingInfo);
                }
            }

            return objModuleFieldInfo.Id;
        }

        public async Task<bool> UpdateFieldPaneAsync(SortPaneFieldsDto data)
        {
            return await _repository.UpdateColumnAsync<ModuleFieldInfo>("PaneName", data.PaneName, data.FieldId);
        }

        public async Task SortFieldsAsync(SortPaneFieldsDto data)
        {
            await _repository.ExecuteStoredProcedureAsync("BusinessEngine_SortModuleFields",
            new
            {
                ModuleId = data.ModuleId,
                PaneName = data.PaneName,
                FieldIds = JsonConvert.SerializeObject(data.PaneFieldIds)
            });

            var cacheKey = AttributeCache.Instance.GetCache<ModuleFieldInfo>().key;
            _cacheService.RemoveByPrefix(cacheKey);
        }

        public async Task<bool> DeleteFieldAsync(Guid id)
        {
            var task1 = _repository.DeleteByScopeAsync<ModuleFieldSettingInfo>(id);
            var task2 = _repository.DeleteAsync<ModuleFieldInfo>(id);

            await Task.WhenAll(task1, task2);

            return await task1 && await task2;
        }

        #endregion

        #region Module Variable Services

        public async Task<IEnumerable<VariableTypeViewModel>> GetVariableTypesViewModelAsync()
        {
            var variableTypes = await _repository.GetAllAsync<VariableTypeInfo>();
            return variableTypes.Select(source =>
            {
                return HybridMapper.MapWithConfig<VariableTypeInfo, VariableTypeViewModel>(
                source, (src, dest) =>
                {
                    dest.Language = (VariableTypeLanguage)source.Language;
                    dest.Icon = (dest.Icon ?? string.Empty).ReplaceFrequentTokens();
                });
            });
        }

        public async Task<IEnumerable<ModuleVariableViewModel>> GetModuleVariablesViewModelAsync(Guid moduleId)
        {
            var task1 = _repository.GetByScopeAsync<ModuleVariableView>(moduleId);
            var task2 = _repository.ExecuteStoredProcedureAsListAsync<ViewModelInfo>("BusinessEngine_GetVariablesViewModels",
                new { ModuleId = moduleId });

            await Task.WhenAll(task1, task2);

            return ModuleMapping.MapModuleVariablesViewModel(await task1, await task2);
        }

        public async Task<ModuleVariableViewModel> GetModuleVariableViewModelAsync(Guid variableId)
        {
            var variable = await _repository.GetAsync<ModuleVariableView>(variableId);

            return ModuleMapping.MapModuleVariableViewModel(variable, null);
        }

        public async Task<Guid> SaveModuleVariablesAsync(ModuleVariableViewModel variale, bool isNew)
        {
            var objModuleVariableInfo = HybridMapper.MapWithConfig<ModuleVariableViewModel, ModuleVariableInfo>(
                variale, (src, dest) => { });

            if (isNew)
            {
                objModuleVariableInfo.Id = await _repository.AddAsync<ModuleVariableInfo>(objModuleVariableInfo, true);
            }
            else
            {
                var isUpdated = await _repository.UpdateAsync<ModuleVariableInfo>(objModuleVariableInfo);
                if (!isUpdated) ErrorService.ThrowUpdateFailedException(objModuleVariableInfo);
            }

            return objModuleVariableInfo.Id;
        }

        public async Task<bool> DeleteModuleVariablesAsync(Guid id)
        {
            return await _repository.DeleteAsync<ModuleVariableInfo>(id);
        }

        #endregion

        #region Module Libraries & Resources

        public async Task<IEnumerable<ModuleCustomLibraryViewModel>> GetModuleCustomLibrariesAsync(Guid moduleId)
        {
            var task1 = _repository.GetByScopeAsync<ModuleCustomLibraryView>(moduleId, "LoadOrder");
            var task2 = _repository.GetByScopeAsync<ModuleCustomLibraryResourceView>(moduleId);

            await Task.WhenAll(task1, task2);

            return null;
            //return DashboardMapping.MapCustomLibrariesViewModel(await task1, await task2);
        }

        public async Task<IEnumerable<ModuleCustomResourceViewModel>> GetModuleCustomResourcesAsync(Guid moduleId)
        {
            var resources = await _repository.GetByScopeAsync<ModuleCustomResourceInfo>(moduleId, "LoadOrder");

            return BaseMapping<ModuleCustomResourceInfo, ModuleCustomResourceViewModel>.MapViewModels(resources);
        }

        public async Task<Guid> SaveModuleCustomLibraryAsync(ModuleCustomLibraryDto library)
        {
            var objModuleCustomLibraryInfo = new ModuleCustomLibraryInfo();
            PropertyCopier<ModuleCustomLibraryDto, ModuleCustomLibraryInfo>.Copy(library, objModuleCustomLibraryInfo);

            if (library.Id == Guid.Empty)
                objModuleCustomLibraryInfo.Id = await _repository.AddAsync<ModuleCustomLibraryInfo>(objModuleCustomLibraryInfo);
            else
            {
                var isUpdated = await _repository.UpdateAsync<ModuleCustomLibraryInfo>(objModuleCustomLibraryInfo);
                if (!isUpdated) ErrorService.ThrowUpdateFailedException(objModuleCustomLibraryInfo);
            }

            return objModuleCustomLibraryInfo.Id;
        }

        public async Task<Guid> SaveModuleCustomResourceAsync(ModuleCustomResourceDto resource)
        {
            var objModuleCustomResourceInfo = new ModuleCustomResourceInfo();
            PropertyCopier<ModuleCustomResourceDto, ModuleCustomResourceInfo>.Copy(resource, objModuleCustomResourceInfo);

            if (resource.Id == Guid.Empty)
                objModuleCustomResourceInfo.Id = await _repository.AddAsync<ModuleCustomResourceInfo>(objModuleCustomResourceInfo);
            else
            {
                var isUpdated = await _repository.UpdateAsync<ModuleCustomResourceInfo>(objModuleCustomResourceInfo);
                if (!isUpdated) ErrorService.ThrowUpdateFailedException(objModuleCustomResourceInfo);
            }

            return objModuleCustomResourceInfo.Id;
        }

        public async Task SortModuleCustomLibraries(LibraryOrResource target, IEnumerable<SortModuleCustomLibrariesDto> items)
        {
            if (target == LibraryOrResource.Library)
                await _repository.ExecuteStoredProcedureAsync("BusinessEngine_SortModuleCustomLibraries",
                    new { JsonData = items.ToJson() });
            else if (target == LibraryOrResource.Resource)
                await _repository.ExecuteStoredProcedureAsync("BusinessEngine_SortModuleCustomResources",
                    new { JsonData = items.ToJson() });

            DataCache.ClearCache("BE_ModuleCustomLibraries_");
            DataCache.ClearCache("BE_ModuleCustomLibraries_Views_");
            DataCache.ClearCache("BE_ModuleCustomResources_");
        }

        public async Task<bool> DeleteModuleCustomLibraryAsync(Guid id)
        {
            return await _repository.DeleteAsync<ModuleCustomLibraryInfo>(id);
        }

        public async Task<bool> DeleteModuleCustomResourceAsync(Guid id)
        {
            return await _repository.DeleteAsync<ModuleCustomResourceInfo>(id);
        }

        #endregion
    }
}
