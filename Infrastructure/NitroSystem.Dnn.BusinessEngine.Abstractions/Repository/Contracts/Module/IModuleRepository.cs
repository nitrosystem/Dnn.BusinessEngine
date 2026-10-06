using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Views.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Procedures.Module;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Module
{
    public interface IModuleRepository
    {
        Task<Guid> AddAsync(ModuleInfo objModuleInfo);

        Task<string> GetModuleNameAsync(Guid id);
        Task<string> GetScenarioNameAsync(Guid id);
        Task<ModuleInfo> GetAsync(Guid id);
        Task<ModuleView> GetViewAsync(Guid id);
        Task<IReadOnlyList<ModuleView>> GetsViewAsync(Guid scenarioId, params string[] columns);
        Task<(
           IReadOnlyList<ModuleFieldSpResult> Fields,
           IReadOnlyList<ModuleFieldDataSourceSpResult> FieldsDataSource,
           IReadOnlyList<ModuleFieldSettingSpResult> FieldsSettings,
           IReadOnlyList<ModuleResourceSpResult> Resources,
           IReadOnlyList<ModuleResourceSpResult> ExternalResources)>
           GetDataForBuildAsync(Guid id);
        ModuleLiteSpResult GetLiteData(int? siteModuleId, Guid? id);

        Task<bool> UpdateAsync(ModuleInfo objModuleInfo, params string[] columns);

        Task<bool> DeleteAsync(Guid id);

        Task<bool> IsRebuildRequiredAsync(Guid id);
    }
}
