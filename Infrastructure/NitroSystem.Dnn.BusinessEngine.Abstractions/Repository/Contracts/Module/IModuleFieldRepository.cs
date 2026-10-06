using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Module;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Module
{
    public interface IModuleFieldRepository
    {
        Task<Guid> AddAsync(ModuleFieldInfo objModuleFieldInfo);

        Task<string> GetFieldTypeAsync(Guid id);
        Task<ModuleFieldInfo> GetAsync(Guid id);
        Task<IReadOnlyList<ModuleFieldInfo>> GetsAsync(Guid moduleId, params string[] columns);

        Task<bool> UpdateAsync(ModuleFieldInfo objModuleFieldInfo, params string[] columns);

        Task<bool> DeleteAsync(Guid id);

        Task SortFieldsAsync(Guid moduleId, string paneName, string fieldIds);
    }
}
