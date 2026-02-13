using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
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
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Export;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Import;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataService.Module
{
    public class ModuleFieldService : IModuleFieldService, IExportable, IImportable
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

        public async Task<string> GetFieldTypeIconAsync(string fieldType)
        {
            return await _repository.GetColumnValueAsync<ModuleFieldTypeInfo, string>("Icon", "FieldType", fieldType);
        }

        public async Task<string> GenerateFieldTypePanesBusinessControllerClassAsync(string fieldType)
        {
            return await _repository.GetColumnValueAsync<ModuleFieldTypeInfo, string>("GeneratePanesBusinessControllerClass", "FieldType", fieldType);
        }

        #endregion

        #region Module Field Services

        public async Task<IEnumerable<ModuleFieldViewModel>> GetFieldsViewModelAsync(Guid moduleId, string sortBy = "ViewOrder")
        {
            var task1 = _repository.GetByScopeAsync<ModuleFieldInfo>(moduleId, sortBy);
            var task2 = _repository.GetChildsByParentColumn<ModuleFieldInfo, ModuleFieldDataSourceInfo>(
                "ModuleId", "FieldId", moduleId);
            var task3 = _repository.GetChildsByParentColumn<ModuleFieldInfo, ModuleFieldSettingInfo>(
                "ModuleId", "FieldId", moduleId);
            var task4 = _repository.GetByScopeAsync<ActionInfo>(moduleId, "ParentId", "ViewOrder");
            var task5 = _repository.GetAllAsync<ActionParamInfo>("ViewOrder");

            await Task.WhenAll(task1, task2, task3, task4, task5);

            var fields = await task1;
            var fieldsDataSource = await task2;
            var fieldsSettings = await task3;
            var actions = await task4;
            var actionParams = await task5;

            var actionList = HybridMapper.MapWithChildren<ActionInfo, ActionListItem, ActionParamInfo, ActionParamListItem>(
                 parents: actions,
                 children: actionParams,
                 parentKeySelector: p => p.Id,
                 childKeySelector: c => c.ActionId,
                 assignChildren: (parent, childs) => parent.Params = childs
             );

            return HybridMapper.MapCollection<ModuleFieldInfo, ModuleFieldViewModel>(fields,
                configAction: (src, dest) =>
                {
                    if (src.HasDataSource)
                        dest.DataSource = HybridMapper.Map<ModuleFieldDataSourceInfo, ModuleFieldDataSourceViewModel>(
                                fieldsDataSource.FirstOrDefault(d => d.FieldId == src.Id) ?? new ModuleFieldDataSourceInfo());

                    var dict = fieldsSettings.GroupBy(c => c.FieldId).ToDictionary(g => g.Key, g => g.AsEnumerable());
                    if (dict.TryGetValue(src.Id, out var settings))
                        dest.Settings = settings.ToDictionary(x => x.SettingName, x => CastingHelper.ConvertStringToObject(x.SettingValue));
                    else
                        dest.Settings = new Dictionary<string, object>();

                    dest.Actions = actionList.Where(a => a.FieldId == dest.Id).ToList();
                }
            );
        }

        public async Task<ModuleFieldViewModel> GetFieldViewModelAsync(Guid fieldId)
        {
            var task1 = _repository.GetAsync<ModuleFieldInfo>(fieldId);
            var task2 = _repository.GetByColumnAsync<ModuleFieldDataSourceInfo>("FieldId", fieldId);
            var task3 = _repository.GetByScopeAsync<ModuleFieldSettingInfo>(fieldId);
            var task4 = _repository.GetItemsByColumnAsync<ActionInfo>("FieldId", fieldId);
            var task5 = _repository.GetAllAsync<ActionParamInfo>("ViewOrder");

            await Task.WhenAll(task1, task2, task3, task4, task5);

            var field = await task1;
            var fieldDataSource = await task2;
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

        public async Task<Guid> SaveFieldAsync(ModuleFieldViewModel field, bool isNew)
        {
            var objModuleFieldInfo = HybridMapper.Map<ModuleFieldViewModel, ModuleFieldInfo>(field);

            _unitOfWork.BeginTransaction();

            try
            {
                if (isNew)
                {
                    objModuleFieldInfo.Id = await _repository.AddAsync<ModuleFieldInfo>(objModuleFieldInfo);
                }
                else
                {
                    var isUpdated = await _repository.UpdateAsync<ModuleFieldInfo>(objModuleFieldInfo);
                    if (!isUpdated) ErrorHandling.ThrowUpdateFailedException(objModuleFieldInfo);

                    await _repository.DeleteByScopeAsync<ModuleFieldSettingInfo>(field.Id);
                }

                if (field.HasDataSource && field.DataSource != null)
                {
                    await _repository.DeleteByScopeAsync<ModuleFieldDataSourceInfo>(field.Id);

                    var objModuleFieldDataSourceInfo = HybridMapper.Map<ModuleFieldDataSourceViewModel, ModuleFieldDataSourceInfo>(field.DataSource,
                        (src, dest) =>
                        {
                            dest.FieldId = objModuleFieldInfo.Id;

                            if (src.Type == ModuleFieldDataSourceType.DefinedList)
                                dest.VariableName = null;
                            else if (src.Type == ModuleFieldDataSourceType.VariableData)
                                dest.ListId = null;
                        });

                    await _repository.AddAsync<ModuleFieldDataSourceInfo>(objModuleFieldDataSourceInfo);
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
            var objModuleFieldInfo = new ModuleFieldInfo()
            {
                Id = data.FieldId,
                ParentId = data.ParentId,
                PaneName = data.PaneName
            };

            return await _repository.UpdateAsync<ModuleFieldInfo>(objModuleFieldInfo, "ParentId", "PaneName");
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

        #region Import Export

        public async Task<ExportResponse> ExportAsync(ExportContext context)
        {
            switch (context.Scope)
            {
                case ImportExportScope.ScenarioFullComponents:
                    var itemss = await GetScenarioFieldsAsync(context.Get<Guid>("ScenarioId"));

                    return new ExportResponse()
                    {
                        Result = itemss,
                        IsSuccess = true
                    };
                case ImportExportScope.Module:
                    var items = await GetModuleFieldsAsync(context.Get<Guid>("ModuleId"));

                    return new ExportResponse()
                    {
                        Result = items,
                        IsSuccess = true
                    };
                default:
                    return null;
            }
        }

        public async Task<ImportResponse> ImportAsync(string json, ImportContext context)
        {
            var items = JsonConvert.DeserializeObject<List<object>>(json);
            var fields = JsonConvert.DeserializeObject<IEnumerable<ModuleFieldInfo>>(items[0].ToString());
            var fieldsDataSource = JsonConvert.DeserializeObject<IEnumerable<ModuleFieldDataSourceInfo>>(items[1].ToString());
            var fieldsSettings = JsonConvert.DeserializeObject<IEnumerable<ModuleFieldSettingInfo>>(items[2].ToString());

            if (context.Scope == ImportExportScope.ScenarioFullComponents)
            {
                await BulkInsertFieldsAndSettingsAsync(fields, fieldsDataSource, fieldsSettings);

            }
            else if (context.Scope == ImportExportScope.Module)
            {
                var moduleId = (Guid)context.DataTrack["ModuleId"];

                await AddFieldsAndSettingsAsync(moduleId, fields, fieldsDataSource, fieldsSettings, context);
            }

            return new ImportResponse()
            {
                IsSuccess = true
            };
        }

        private async Task<object> GetScenarioFieldsAsync(Guid scenarioId)
        {
            var fields = new List<ModuleFieldInfo>();
            var fieldsDataSources = new List<ModuleFieldDataSourceInfo>();
            var fieldsSettings = new List<ModuleFieldSettingInfo>();

            var moduleIds = await _repository.GetColumnsValueAsync<ModuleInfo, Guid>("Id", "ScenarioId", scenarioId);
            foreach (var moduleId in moduleIds)
            {
                var flds = await _repository.GetByScopeAsync<ModuleFieldInfo>(moduleId);
                fields.AddRange(flds);

                foreach (var field in flds)
                {
                    fieldsDataSources.Add(await _repository.GetByColumnAsync<ModuleFieldDataSourceInfo>("FieldId", field.Id));
                    fieldsSettings.AddRange(await _repository.GetByScopeAsync<ModuleFieldSettingInfo>(field.Id));
                }
            }

            return new List<object>() { fields, fieldsDataSources, fieldsSettings };
        }

        private async Task<object> GetModuleFieldsAsync(Guid moduleId)
        {
            var fields = await _repository.GetByScopeAsync<ModuleFieldInfo>(moduleId);
            var fieldsDataSources = new List<ModuleFieldDataSourceInfo>();
            var fieldsSettings = new List<ModuleFieldSettingInfo>();

            foreach (var field in fields)
            {
                fieldsDataSources.Add(await _repository.GetByColumnAsync<ModuleFieldDataSourceInfo>("FieldId", field.Id));
                fieldsSettings.AddRange(await _repository.GetByScopeAsync<ModuleFieldSettingInfo>(field.Id));
            }

            return new List<object>() { fields, fieldsDataSources, fieldsSettings };
        }

        private async Task AddFieldsAndSettingsAsync(Guid moduleId, IEnumerable<ModuleFieldInfo> fields, IEnumerable<ModuleFieldDataSourceInfo> fieldsDataSources, IEnumerable<ModuleFieldSettingInfo> fieldsSettings, ImportContext context)
        {
            var fieldList = fields.ToList();

            var inserted = new HashSet<Guid>();
            var remaining = new List<ModuleFieldInfo>(fieldList);

            while (remaining.Any())
            {
                var batch = remaining
                    .Where(f => f.ParentId == null || inserted.Contains(f.ParentId.Value))
                    .ToList();

                if (!batch.Any())
                    throw new InvalidOperationException(
                        "Circular reference or missing parent detected in ModuleFieldInfo.");

                foreach (var field in batch)
                {
                    var oldFieldId = field.Id;

                    field.ModuleId = moduleId;
                    field.Id = Guid.NewGuid();

                    if (field.ParentId.HasValue && context.DataTrack.TryGetValue(field.ParentId.Value.ToString(), out var parentId))
                        field.ParentId = (Guid)parentId;

                    await _repository.AddAsync<ModuleFieldInfo>(field);

                    context.DataTrack.Add(oldFieldId.ToString(), field.Id);
                }

                foreach (var f in batch)
                    inserted.Add(f.Id);

                remaining.RemoveAll(f => inserted.Contains(f.Id));
            }

            foreach (var datasource in fieldsDataSources)
            {
                datasource.FieldId = (Guid)context.DataTrack[datasource.FieldId.ToString()];
                datasource.Id = Guid.NewGuid();

                await _repository.AddAsync<ModuleFieldDataSourceInfo>(datasource);
            }

            foreach (var setting in fieldsSettings)
            {
                setting.FieldId = (Guid)context.DataTrack[setting.FieldId.ToString()];
                setting.Id = Guid.NewGuid();

                await _repository.AddAsync<ModuleFieldSettingInfo>(setting);
            }
        }

        private async Task BulkInsertFieldsAndSettingsAsync(IEnumerable<ModuleFieldInfo> fields, IEnumerable<ModuleFieldDataSourceInfo> fieldsDataSource, IEnumerable<ModuleFieldSettingInfo> fieldsSettings)
        {
            var fieldList = fields.ToList();

            var inserted = new HashSet<Guid>();
            var remaining = new List<ModuleFieldInfo>(fieldList);

            while (remaining.Any())
            {
                var batch = remaining
                    .Where(f => f.ParentId == null || inserted.Contains(f.ParentId.Value))
                    .ToList();

                if (!batch.Any())
                    throw new InvalidOperationException(
                        "Circular reference or missing parent detected in ModuleFieldInfo.");

                await _repository.BulkInsertAsync(batch);

                foreach (var f in batch)
                    inserted.Add(f.Id);

                remaining.RemoveAll(f => inserted.Contains(f.Id));
            }

            await _repository.BulkInsertAsync(fieldsDataSource);
            await _repository.BulkInsertAsync(fieldsSettings);
        }

        #endregion
    }
}
