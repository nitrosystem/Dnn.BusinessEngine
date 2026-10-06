using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Module;

namespace NitroSystem.Dnn.BusinessEngine.Repository.Module
{
    public class ModuleOutputResourceRepository: IModuleOutputResourceRepository
    {
        private readonly ISql _sql;

        public ModuleOutputResourceRepository(ISql sql)
        {
            _sql = sql;
        }

        public async Task BulkInsertAsync(IReadOnlyList<ModuleOutputResourceInfo> resources)
        {
            await _sql.BulkInsertAsync<ModuleOutputResourceInfo>(resources);
        }

        public async Task DeleteAsync(Guid moduleId)
        {
            await _sql.DeleteByScopeAsync<ModuleOutputResourceInfo>(moduleId);
        }

        #region Stored Procedures

        public IReadOnlyList<ModuleOutputResourceInfo> Gets(int sitePageId, string moduleIds)
        {
            return _sql.ExecuteStoredProcedureAsList<ModuleOutputResourceInfo>(
              "dbo.BusinessEngine_App_GetModuleOutputResources", "Be_Modules_App_LiteData",
              new
              {
                  PageId = sitePageId,
                  ModuleIds = moduleIds
              });
        }

        #endregion
    }
}
