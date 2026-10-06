using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Views.Module;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Module
{
    public interface IModuleCustomLibraryRepository
    {
        Task<Guid> AddAsync(ModuleCustomLibraryInfo objModuleCustomLibraryInfo);

        Task<ModuleCustomLibraryInfo> GetAsync(Guid id);
        Task<IReadOnlyList<ModuleCustomLibraryInfo>> GetsAsync(Guid moduleId, params string[] columns);
        Task<IReadOnlyList<ModuleCustomLibraryView>> GetsViewAsync(Guid moduleId, params string[] columns);
        Task<IReadOnlyList<ModuleCustomLibraryResourceView>> GetsResourceViewAsync(Guid moduleId, params string[] columns);

        Task<bool> UpdateAsync(ModuleCustomLibraryInfo objModuleCustomLibraryInfo, params string[] columns);

        Task<bool> DeleteAsync(Guid id);
    }
}
