using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Module;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Module
{
    public interface IModuleFieldTypeRepository
    {
        Task<Guid> AddAsync(ModuleFieldTypeInfo objModuleFieldTypeInfo);

        Task<string> GetIconAsync(string fieldType);
        Task<string> GetGeneratePanesBusinessControllerClassAsync(string fieldType);
        Task<ModuleFieldTypeInfo> GetAsync(Guid id);
        Task<IReadOnlyList<ModuleFieldTypeInfo>> GetsAsync(params string[] columns);

        Task<bool> UpdateAsync(ModuleFieldTypeInfo objModuleFieldTypeInfo, params string[] columns);

        Task<bool> DeleteAsync(Guid id);

        Task<bool> HasFieldTypeAsync();
    }
}
