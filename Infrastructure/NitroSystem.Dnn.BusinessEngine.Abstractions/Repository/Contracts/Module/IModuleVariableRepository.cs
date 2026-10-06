using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.AppModel;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Procedures.Module;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Module
{
    public interface IModuleVariableRepository
    {
        Task<Guid> AddAsync(ModuleVariableInfo objModuleVariableInfo);

        Task<string> GetVariableNameAsync(Guid id);
        Task<IReadOnlyList<ModuleVariableInfo>> GetsAsync(Guid moduleId, params string[] columns);
        Task<(IReadOnlyList<ModuleVariableSpResult> Variables, IReadOnlyList<AppModelPropertyInfo> Properties)> GetsAsync(
            Guid moduleId,
            ModuleVariableScope fromScope,
            ModuleVariableScope toScope);

        Task<bool> UpdateAsync(ModuleVariableInfo objModuleVariableInfo, params string[] columns);

        Task<bool> DeleteAsync(Guid id);
    }
}
