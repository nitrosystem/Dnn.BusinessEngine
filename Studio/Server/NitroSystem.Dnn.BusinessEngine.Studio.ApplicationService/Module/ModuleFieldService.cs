using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.DefinedList;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Action;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Action;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Module;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;

namespace NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.Module
{
    public class ModuleFieldService : IModuleFieldService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly IModuleFieldTypeRepository _moduleFieldTypeRepository;
        private readonly IModuleFieldTypeTemplateRepository _moduleFieldTypeTemplateRepository;
        private readonly IModuleFieldTypeThemeRepository _moduleFieldTypeThemeRepository;
        private readonly IModuleEventTypeRepository _moduleEventTypeRepository;
        private readonly IModuleFieldRepository _moduleFieldRepository;
        private readonly IModuleFieldDataSourceRepository _moduleFieldDataSourceRepository;
        private readonly IModuleFieldSettingRepository _moduleFieldSettingRepository;
        private readonly IActionRepository _actionRepository;
        private readonly IActionParamRepository _actionParamRepository;
        private readonly IDefinedListItemRepository _definedListItemRepository;

        public ModuleFieldService(
            IUnitOfWork unitOfWork,
            ICacheService cacheService,
            IModuleFieldTypeRepository moduleFieldTypeRepository,
            IModuleFieldTypeTemplateRepository moduleFieldTypeTemplateRepository,
            IModuleFieldTypeThemeRepository moduleFieldTypeThemeRepository,
            IModuleEventTypeRepository moduleEventTypeRepository,
            IModuleFieldRepository moduleFieldRepository,
            IModuleFieldDataSourceRepository moduleFieldDataSourceRepository,
            IModuleFieldSettingRepository moduleFieldSettingRepository,
            IActionRepository actionRepository,
            IActionParamRepository actionParamRepository,
            IDefinedListItemRepository definedListItemRepository)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _moduleFieldTypeRepository = moduleFieldTypeRepository;
            _moduleFieldTypeTemplateRepository = moduleFieldTypeTemplateRepository;
            _moduleFieldTypeThemeRepository = moduleFieldTypeThemeRepository;
            _moduleEventTypeRepository = moduleEventTypeRepository;
            _moduleFieldRepository = moduleFieldRepository;
            _moduleFieldDataSourceRepository = moduleFieldDataSourceRepository;
            _moduleFieldSettingRepository = moduleFieldSettingRepository;
            _actionRepository = actionRepository;
            _actionParamRepository = actionParamRepository;
            _definedListItemRepository = definedListItemRepository;
        }

        #region Module Field Type Services

        public async Task<IEnumerable<ModuleFieldTypeViewModel>> GetFieldTypesViewModelAsync()
        {
            var task1 = _moduleFieldTypeRepository.GetsAsync("FieldDomain", "ViewOrder");
            var task2 = _moduleFieldTypeTemplateRepository.GetsAsync("ViewOrder");
            var task3 = _moduleFieldTypeThemeRepository.GetsAsync("ViewOrder");
            var task4 = _moduleEventTypeRepository.GetsAsync();

            await Task.WhenAll(task1, task2, task3, task4);

            var fieldsTypes = await task1;
            var templates = await task2;
            var themes = await task3;
            var events = await task4;

            var builder = new CollectionMappingBuilder<ModuleFieldTypeInfo, ModuleFieldTypeViewModel>();

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

            var result = await builder.BuildAsync(fieldsTypes, (src, dest) =>
            {
                dest.CanHaveAction = events.Any(e => e.Component == src.FieldType);
            });
            return result;
        }

        public async Task<string> GetFieldTypeIconAsync(string fieldType)
        {
            return await _moduleFieldTypeRepository.GetIconAsync(fieldType);
        }

        public async Task<string> GetGeneratePanesBusinessControllerClassAsync(string fieldType)
        {
            return await _moduleFieldTypeRepository.GetGeneratePanesBusinessControllerClassAsync(fieldType);
        }

        public async Task<bool> HasFieldTypeAsync()
        {
            return await _moduleFieldTypeRepository.HasFieldTypeAsync();
        }

        #endregion

        #region Module Field Services

        public async Task<string> GetFieldTypeAsync(Guid fieldId)
        {
            return await _moduleFieldRepository.GetFieldTypeAsync(fieldId);
        }

        public async Task<ModuleFieldViewModel> GetFieldViewModelAsync(Guid fieldId)
        {
            var task1 = _moduleFieldRepository.GetAsync(fieldId);
            var task2 = _moduleFieldDataSourceRepository.GetsAsync(fieldId);
            var task3 = _moduleFieldSettingRepository.GetsAsync(fieldId);
            var task4 = _actionRepository.GetsByFieldIdAsync(fieldId);
            var task5 = _actionParamRepository.GetsAsync("ViewOrder");

            await Task.WhenAll(task1, task2, task3, task4, task5);

            var field = await task1;
            var fieldDataSource = (await task2)?.First();
            var fieldClientBehaviors = await task3;
            var fieldSettings = await task3;
            var actions = await task4;
            var actionParams = await task5;

            var actionList = HybridMapper.MapWithChildren<ActionInfo, ActionListItem, ActionParamInfo, ActionParamListItem>(
                 parents: actions,
                 children: actionParams,
                 parentKeySelector: p => p.Id,
                 childKeySelector: c => c.ActionId,
                 assignChildren: (parent, childs) => parent.Params = childs
             );

            return HybridMapper.MapWithChildren<ModuleFieldInfo, ModuleFieldViewModel, ActionInfo, ActionListItem>(
                  source: field,
                  children: actions,
                  assignChildren: (parent, childs) => parent.Actions = childs,
                  moreAssigns: (src, dest) =>
                  {
                      if (src.HasDataSource)
                          dest.DataSource = HybridMapper.Map<ModuleFieldDataSourceInfo, ModuleFieldDataSourceViewModel>(
                                  fieldDataSource ?? new ModuleFieldDataSourceInfo());

                      dest.Settings = fieldSettings.ToDictionary(x => x.SettingName, x => CastingHelper.ConvertStringToObject(x.SettingValue));
                      dest.Actions = actionList.Where(a => a.FieldId == dest.Id).ToList();
                  }
            );
        }

        public async Task<IEnumerable<ModuleFieldViewModel>> GetFieldsViewModelAsync(Guid moduleId, string sortBy = "ViewOrder")
        {
            var task1 = _moduleFieldRepository.GetsAsync(moduleId, sortBy);
            var task2 = _moduleFieldDataSourceRepository.GetsByAncestorAsync(moduleId);
            var task3 = _moduleFieldSettingRepository.GetsByAncestorAsync(moduleId);
            var task4 = _actionRepository.GetsAsync(moduleId, "ParentId", "ExecuteOrder");
            var task5 = _actionParamRepository.GetsByAncestorAsync(moduleId, "ViewOrder");
            var task6 = _moduleEventTypeRepository.GetsAsync();

            await Task.WhenAll(task1, task2, task3, task4, task5, task6);

            var fields = await task1;
            var fieldsDataSource = await task2;
            var fieldsSettings = await task3;
            var actions = await task4;
            var actionParams = await task5;
            var events = await task6;

            var actionList = HybridMapper.MapWithChildren<ActionInfo, ActionListItem, ActionParamInfo, ActionParamListItem>(
                 parents: actions,
                 children: actionParams,
                 parentKeySelector: p => p.Id,
                 childKeySelector: c => c.ActionId,
                 assignChildren: (parent, childs) => parent.Params = childs
             );

            return await HybridMapper.MapCollectionAsync<ModuleFieldInfo, ModuleFieldViewModel>(fields,
                async (src, dest) =>
                {
                    if (src.HasDataSource)
                        dest.DataSource = await GetFieldDataSource(fieldsDataSource.FirstOrDefault(d => d.FieldId == src.Id));

                    var dict = fieldsSettings.GroupBy(c => c.FieldId).ToDictionary(g => g.Key, g => g.AsEnumerable());
                    if (dict.TryGetValue(src.Id, out var settings))
                        dest.Settings = settings.ToDictionary(x => x.SettingName, x => CastingHelper.ConvertStringToObject(x.SettingValue));
                    else
                        dest.Settings = new Dictionary<string, object>();

                    dest.CanHaveAction = events.Any(e => e.Component == src.FieldType);
                    dest.Actions = actionList.Where(a => a.FieldId == dest.Id).ToList();
                }
            );
        }

        public async Task<IEnumerable<ModuleFieldListItem>> GetFieldsListItemAsync(Guid moduleId, string sortBy)
        {
            var fields = await _moduleFieldRepository.GetsAsync(moduleId, sortBy);
            return HybridMapper.MapCollection<ModuleFieldInfo, ModuleFieldListItem>(fields);
        }

        public async Task<ModuleFieldDataSourceViewModel> GetFieldDataSource(ModuleFieldDataSourceInfo objModuleFieldDataSourceInfo)
        {
            var dataSource = HybridMapper.Map<ModuleFieldDataSourceInfo, ModuleFieldDataSourceViewModel>(objModuleFieldDataSourceInfo ?? new ModuleFieldDataSourceInfo());
            if (dataSource.Type == ModuleFieldDataSourceType.DefinedList && dataSource.ListId.HasValue)
                dataSource.Items = await _definedListItemRepository.GetsAsync(dataSource.ListId.Value, "ViewOrder");

            return dataSource;
        }

        public async Task<Guid> CreateFieldAsync(ModuleFieldViewModel field)
        {
            var objModuleFieldInfo = HybridMapper.Map<ModuleFieldViewModel, ModuleFieldInfo>(field);

            _unitOfWork.BeginTransaction();

            try
            {
                if (objModuleFieldInfo.Id == Guid.Empty)
                {
                    objModuleFieldInfo.Id = await _moduleFieldRepository.AddAsync(objModuleFieldInfo);
                }
                else
                {
                    await _moduleFieldRepository.UpdateAsync(objModuleFieldInfo);

                    //Delete old FieldSettings
                    await _moduleFieldSettingRepository.DeletesAsync(field.Id);
                }

                //Field Data Source
                if (field.HasDataSource && field.DataSource != null)
                {
                    await _moduleFieldDataSourceRepository.DeletesAsync(field.Id);

                    var objModuleFieldDataSourceInfo = HybridMapper.Map<ModuleFieldDataSourceViewModel, ModuleFieldDataSourceInfo>(field.DataSource,
                        (src, dest) =>
                        {
                            dest.FieldId = objModuleFieldInfo.Id;

                            if (src.Type == ModuleFieldDataSourceType.DefinedList)
                                dest.VariableName = null;
                            else if (src.Type == ModuleFieldDataSourceType.VariableData)
                                dest.ListId = null;
                        });

                    await _moduleFieldDataSourceRepository.AddAsync(objModuleFieldDataSourceInfo);
                }

                //Field Settings
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

                        await _moduleFieldSettingRepository.AddAsync(objModuleFieldSettingInfo);
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
            var objModuleFieldInfo = new ModuleFieldInfo()
            {
                Id = data.FieldId,
                ParentId = data.ParentId,
                PaneName = data.PaneName,
            };

            return await _moduleFieldRepository.UpdateAsync(objModuleFieldInfo, "ParentId", "PaneName");
        }

        public async Task SortFieldsAsync(PaneFieldsOrder data)
        {
            var fieldIds = JsonConvert.SerializeObject(data.PaneFieldIds);
            await _moduleFieldRepository.SortFieldsAsync(data.ModuleId, data.PaneName, fieldIds);
        }

        public async Task<bool> DeleteFieldAsync(Guid moduleId)
        {
            _unitOfWork.BeginTransaction();

            try
            {
                await _moduleFieldRepository.DeleteAsync(moduleId);
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
