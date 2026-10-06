using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Module;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Module
{
    public interface IModuleFieldTypeTemplateRepository
    {
        Task<Guid> AddAsync(ModuleFieldTypeTemplateInfo objModuleFieldTypeTemplateInfo);

        Task<ModuleFieldTypeTemplateInfo> GetAsync(Guid id);
        Task<IReadOnlyList<ModuleFieldTypeTemplateInfo>> GetsAsync(params string[] columns);

        Task<bool> UpdateAsync(ModuleFieldTypeTemplateInfo objModuleFieldTypeTemplateInfo, params string[] columns);

        Task<bool> DeleteAsync(Guid id);
    }
}
