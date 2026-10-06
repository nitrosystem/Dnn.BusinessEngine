using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Module;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Module
{
    public interface IModuleFieldTypeThemeRepository
    {
        Task<Guid> AddAsync(ModuleFieldTypeThemeInfo objModuleFieldTypeThemeInfo);
        Task<ModuleFieldTypeThemeInfo> GetAsync(Guid id);
        Task<IReadOnlyList<ModuleFieldTypeThemeInfo>> GetsAsync(params string[] columns);

        Task<bool> UpdateAsync(ModuleFieldTypeThemeInfo objModuleFieldTypeThemeInfo, params string[] columns);

        Task<bool> DeleteAsync(Guid id);
    }
}
