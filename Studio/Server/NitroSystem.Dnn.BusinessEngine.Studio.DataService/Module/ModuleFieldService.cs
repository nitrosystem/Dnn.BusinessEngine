using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Data.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Module;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Security;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Views;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataService.Module
{
    public class ModuleFieldService : IModuleFieldService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly IRepositoryBase _repository;

        public ModuleFieldService(IUnitOfWork unitOfWork, ICacheService cacheService, IRepositoryBase repository)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _repository = repository;
        }

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

        public async Task<bool> UpdateFieldPaneAsync(PaneFieldsOrder data)
        {
            return await _repository.UpdateColumnAsync<ModuleFieldInfo>("PaneName", data.PaneName, data.FieldId);
        }

        public async Task SortFieldsAsync(PaneFieldsOrder data)
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
    }
}
