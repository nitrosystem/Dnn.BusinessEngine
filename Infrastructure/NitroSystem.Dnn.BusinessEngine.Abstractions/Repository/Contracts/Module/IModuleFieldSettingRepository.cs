using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Module;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Module
{
    public interface IModuleFieldSettingRepository
    {
        Task<Guid> AddAsync(ModuleFieldSettingInfo objModuleFieldSettingInfo);

        Task<ModuleFieldSettingInfo> GetAsync(Guid id);
        Task<IReadOnlyList<ModuleFieldSettingInfo>> GetsByAncestorAsync(Guid moduleId);
        Task<IReadOnlyList<ModuleFieldSettingInfo>> GetsAsync(Guid fieldId, params string[] columns);

        Task<bool> UpdateAsync(ModuleFieldSettingInfo objModuleFieldSettingInfo, params string[] columns);

        Task<bool> DeleteAsync(Guid id);
        Task<bool> DeletesAsync(Guid fieldId);
    }
}
