using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.DefinedList;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;

namespace NitroSystem.Dnn.BusinessEngine.App.ApplicationService.Module
{
    public class ModuleFieldService : IModuleFieldService
    {
        private readonly IModuleFieldRepository _moduleFieldRepository;
        private readonly IModuleFieldDataSourceRepository _moduleFieldDataSourceRepository;
        private readonly IModuleFieldSettingRepository _moduleFieldSettingRepository;
        private readonly IDefinedListItemRepository _definedListItemRepository;

        public ModuleFieldService(
            IModuleFieldRepository moduleFieldRepository,
            IModuleFieldDataSourceRepository moduleFieldDataSourceRepository,
            IModuleFieldSettingRepository moduleFieldSettingRepository,
            IDefinedListItemRepository definedListItemRepository)
        {
            _moduleFieldRepository = moduleFieldRepository;
            _moduleFieldDataSourceRepository = moduleFieldDataSourceRepository;
            _moduleFieldSettingRepository = moduleFieldSettingRepository;
            _definedListItemRepository = definedListItemRepository;
        }

        public async Task<IEnumerable<ModuleFieldDto>> GetFieldsDtoAsync(Guid moduleId)
        {
            var fields = await _moduleFieldRepository.GetsAsync(moduleId);
            var fieldsDataSource = await _moduleFieldDataSourceRepository.GetsByAncestorAsync(moduleId);
            var fieldsSettings = await _moduleFieldSettingRepository.GetsByAncestorAsync(moduleId);

            return await HybridMapper.MapCollectionAsync<ModuleFieldInfo, ModuleFieldDto>(fields,
                async (src, dest) =>
                {
                    if (src.HasDataSource)
                        dest.DataSource = await GetFieldDataSource(fieldsDataSource.FirstOrDefault(d => d.FieldId == src.Id));

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
            var field = await _moduleFieldRepository.GetAsync(fieldId);
            var fieldSettings = await _moduleFieldSettingRepository.GetsAsync(fieldId);

            return await HybridMapper.MapAsync<ModuleFieldInfo, ModuleFieldDto>(field,
                async (src, dest) =>
                {
                    if (includeDataSource && src.HasDataSource)
                        dest.DataSource = await GetFieldDataSource(src.Id);

                    dest.Settings = fieldSettings.ToDictionary(x => x.SettingName, x => CastingHelper.ConvertStringToObject(x.SettingValue));
                }
            );
        }

        public async Task<ModuleFieldDto> GetFieldDtoAsync(Guid fieldId, bool includeDataSource = false, bool includeSettings = false)
        {
            var field = await _moduleFieldRepository.GetAsync(fieldId);
            var fieldsSettings = await _moduleFieldSettingRepository.GetsAsync(fieldId);
            var fieldDataSource = (await _moduleFieldDataSourceRepository.GetsAsync(fieldId))?.First();

            return await HybridMapper.MapAsync<ModuleFieldInfo, ModuleFieldDto>(field,
                async (src, dest) =>
                {
                    if (includeDataSource && src.HasDataSource && fieldDataSource != null)
                        dest.DataSource = await GetFieldDataSource(fieldDataSource);

                    if (includeSettings)
                    {
                        var dict = fieldsSettings.GroupBy(c => c.FieldId).ToDictionary(g => g.Key, g => g.AsEnumerable());
                        if (dict.TryGetValue(src.Id, out var settings))
                            dest.Settings = settings.ToDictionary(x => x.SettingName, x => CastingHelper.ConvertStringToObject(x.SettingValue));
                        else
                            dest.Settings = new Dictionary<string, object>();
                    }
                }
            );
        }

        public async Task<ModuleFieldDataSourceDto> GetFieldDataSource(Guid fieldId)
        {
            var dataSource = (await _moduleFieldDataSourceRepository.GetsAsync(fieldId)).First();
            return dataSource != null
                ? await GetFieldDataSource(dataSource)
                : null;
        }

        public async Task<ModuleFieldDataSourceDto> GetFieldDataSource(ModuleFieldDataSourceInfo objModuleFieldDataSourceInfo)
        {
            var dataSource = HybridMapper.Map<ModuleFieldDataSourceInfo, ModuleFieldDataSourceDto>(objModuleFieldDataSourceInfo ?? new ModuleFieldDataSourceInfo());
            if (dataSource.Type == ModuleFieldDataSourceType.DefinedList && dataSource.ListId.HasValue)
                dataSource.Items = await _definedListItemRepository.GetsAsync(dataSource.ListId.Value, "ViewOrder");

            return dataSource;
        }
    }
}
