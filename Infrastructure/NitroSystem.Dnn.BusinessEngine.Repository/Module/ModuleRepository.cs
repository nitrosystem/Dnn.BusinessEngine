using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Views.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Procedures.Module;

namespace NitroSystem.Dnn.BusinessEngine.Repository.Module
{
    public class ModuleRepository : IModuleRepository
    {
        private readonly ISql _sql;

        public ModuleRepository(ISql sql)
        {
            _sql = sql;
        }

        public async Task<Guid> AddAsync(ModuleInfo objModuleInfo)
        {
            return await _sql.InsertAsync<ModuleInfo>(objModuleInfo);
        }

        public async Task<string> GetModuleNameAsync(Guid id)
        {
            return await _sql.GetColumnValueAsync<ModuleInfo, string>(id, "ModuleName");
        }

        public async Task<string> GetScenarioNameAsync(Guid id)
        {
            return await _sql.GetColumnValueAsync<ModuleView, string>(id, "ScenarioName");
        }

        public async Task<ModuleInfo> GetAsync(Guid id)
        {
            return await _sql.GetAsync<ModuleInfo>(id);
        }

        public async Task<ModuleView> GetViewAsync(Guid id)
        {
            return await _sql.GetAsync<ModuleView>(id);
        }

        public async Task<IReadOnlyList<ModuleView>> GetsViewAsync(Guid scenarioId, params string[] columns)
        {
            return await _sql.GetByScopeAsync<ModuleView>(scenarioId, columns);
        }

        public async Task<bool> UpdateAsync(ModuleInfo objModuleInfo, params string[] columns)
        {
            return await _sql.UpdateAsync<ModuleInfo>(objModuleInfo, columns);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _sql.DeleteAsync<ModuleInfo>(id);
        }

        #region Stored Procedures

        public async Task<(
            IReadOnlyList<ModuleFieldSpResult> Fields,
            IReadOnlyList<ModuleFieldDataSourceSpResult> FieldsDataSource,
            IReadOnlyList<ModuleFieldSettingSpResult> FieldsSettings,
            IReadOnlyList<ModuleResourceSpResult> Resources,
            IReadOnlyList<ModuleResourceSpResult> ExternalResources)>
            GetDataForBuildAsync(Guid id)
        {
            return await _sql.ExecuteStoredProcedureMultipleAsync<
                ModuleFieldSpResult,
                ModuleFieldDataSourceSpResult,
                ModuleFieldSettingSpResult,
                ModuleResourceSpResult,
                ModuleResourceSpResult>(
                "dbo.BusinessEngine_Studio_GetModuleDataForBuild",
                "BE_Modules_Fields_Settings_Studio_GetModuleDataForBuild_",
                new
                {
                    ModuleId = id
                });
        }

        public async Task<bool> IsRebuildRequiredAsync(Guid id)
        {
            return await _sql.ExecuteStoredProcedureScalerAsync<bool>("dbo.BusinessEngine_Studio_IsRebuildRequired", "",
                new
                {
                    ModuleId = id
                });
        }

        public ModuleLiteSpResult GetLiteData(int? siteModuleId, Guid? id)
        {
            return _sql.ExecuteStoredProcedure<ModuleLiteSpResult>(
                "dbo.BusinessEngine_App_GetModuleLite", "Be_Modules_App_LiteData",
                new
                {
                    SiteModuleId = siteModuleId,
                    ModuleId = id
                });
        }

        #endregion
    }
}
