using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.AppModel;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Procedures.Module;

namespace NitroSystem.Dnn.BusinessEngine.Repository.Module
{
    public class ModuleVariableRepository: IModuleVariableRepository
    {
        private readonly ISql _sql;

        public ModuleVariableRepository(ISql sql)
        {
            _sql = sql;
        }

        public async Task<Guid> AddAsync(ModuleVariableInfo objModuleVariableInfo)
        {
            return await _sql.InsertAsync<ModuleVariableInfo>(objModuleVariableInfo);
        }

        public async Task<string> GetVariableNameAsync(Guid id)
        {
            return await _sql.GetColumnValueAsync<ModuleVariableInfo, string>(id, "ModuleVariableName");
        }

        public async Task<IReadOnlyList<ModuleVariableInfo>> GetsAsync(Guid moduleId, params string[] columns)
        {
            return await _sql.GetByScopeAsync<ModuleVariableInfo>(moduleId, columns);
        }

        public async Task<bool> UpdateAsync(ModuleVariableInfo objModuleVariableInfo, params string[] columns)
        {
            return await _sql.UpdateAsync<ModuleVariableInfo>(objModuleVariableInfo, columns);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _sql.DeleteAsync<ModuleVariableInfo>(id);
        }

        #region Stored Procedures

        public async Task<(IReadOnlyList<ModuleVariableSpResult> Variables, IReadOnlyList<AppModelPropertyInfo> Properties)> GetsAsync(
            Guid moduleId,
            ModuleVariableScope fromScope,
            ModuleVariableScope toScope)
        {
            return await _sql.ExecuteStoredProcedureMultipleAsync<ModuleVariableSpResult, AppModelPropertyInfo>(
                "dbo.BusinessEngine_App_GetModuleVariables", "BE_Modules_Variables_App_",
                    new
                    {
                        ModuleId = moduleId,
                        FromScope = fromScope,
                        ToScope = toScope,
                    }
                );
        }

        #endregion
    }
}
