using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.AppModel;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.AppModel;

namespace NitroSystem.Dnn.BusinessEngine.Repository.AppModel
{
    public class AppModelRepository : IAppModelRepository
    {
        private readonly ISql _sql;

        public AppModelRepository(ISql sql)
        {
            _sql = sql;
        }

        public async Task<Guid> AddAsync(AppModelInfo objAppModelInfo)
        {
            return await _sql.InsertAsync<AppModelInfo>(objAppModelInfo);
        }

        public async Task<AppModelInfo> GetAsync(Guid id)
        {
            return await _sql.GetAsync<AppModelInfo>(id);
        }

        public async Task<IReadOnlyList<AppModelInfo>> GetsAsync(Guid scenarioId, params string[] columns)
        {
            return await _sql.GetByScopeAsync<AppModelInfo>(scenarioId, columns);
        }

        public async Task UpdateGroupAsync(Guid? groupId, Guid entityId)
        {
            await _sql.UpdateColumnAsync<AppModelInfo>("GroupId", groupId, entityId);
        }

        public async Task<bool> UpdateAsync(AppModelInfo objAppModelInfo, params string[] columns)
        {
            return await _sql.UpdateAsync<AppModelInfo>(objAppModelInfo, columns);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _sql.DeleteAsync<AppModelInfo>(id);
        }

        #region Stored Procedures

        public async Task<(IReadOnlyList<AppModelInfo> Models, IReadOnlyList<AppModelPropertyInfo> Properties, int? TotalCount)>
            GetsAsync(
            Guid scenarioId,
            int pageIndex,
            int pageSize,
            string searchText,
            AppModelType? modelType,
            string sortBy)
        {
            var results = await _sql.ExecuteStoredProcedureMultipleAsync<int?, AppModelInfo, AppModelPropertyInfo>(
                    "dbo.BusinessEngine_Studio_GetAppModelsWithProperties", "BE_AppModels_Studio_GetAppModelsWithProperties_",
                    new
                    {
                        ScenarioId = scenarioId,
                        SearchText = searchText,
                        ModelType = modelType,
                        PageIndex = pageIndex,
                        PageSize = pageSize,
                        SortBy = sortBy
                    });

            var totalCount = results.Item1.Any()
                ? results.Item1.First()
                : 0;

            return (results.Item2, results.Item3, totalCount);
        }

        public async Task<IReadOnlyList<AppModelPropertyInfo>> GetsAsModuleVariables(Guid moduleId)
        {
            return await _sql.ExecuteStoredProcedureAsListAsync<AppModelPropertyInfo>(
                "dbo.BusinessEngine_Studio_GetAppModelPropertiesAsModuleVariables", "BE_Modules_Variables__Studio_AppModelProps",
            new
            {
                ModuleId = moduleId
            });
        }


        #endregion
    }
}
