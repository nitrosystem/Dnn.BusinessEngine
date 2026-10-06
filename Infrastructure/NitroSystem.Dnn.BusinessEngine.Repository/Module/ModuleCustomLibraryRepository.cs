using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Views.Module;

namespace NitroSystem.Dnn.BusinessEngine.Repository.Module
{
    public class ModuleCustomLibraryRepository : IModuleCustomLibraryRepository
    {
        private readonly ISql _sql;

        public ModuleCustomLibraryRepository(ISql sql)
        {
            _sql = sql;
        }

        public async Task<Guid> AddAsync(ModuleCustomLibraryInfo objModuleCustomLibraryInfo)
        {
            return await _sql.InsertAsync<ModuleCustomLibraryInfo>(objModuleCustomLibraryInfo);
        }

        public async Task<ModuleCustomLibraryInfo> GetAsync(Guid id)
        {
            return await _sql.GetAsync<ModuleCustomLibraryInfo>(id);
        }

        public async Task<IReadOnlyList<ModuleCustomLibraryInfo>> GetsAsync(Guid moduleId, params string[] columns)
        {
            return await _sql.GetByScopeAsync<ModuleCustomLibraryInfo>(moduleId, columns);
        }

        public async Task<IReadOnlyList<ModuleCustomLibraryView>> GetsViewAsync(Guid moduleId, params string[] columns)
        {
            return await _sql.GetByScopeAsync<ModuleCustomLibraryView>(moduleId, columns);
        }

        public async Task<IReadOnlyList<ModuleCustomLibraryResourceView>> GetsResourceViewAsync(Guid moduleId, params string[] columns)
        {
            return await _sql.GetByScopeAsync<ModuleCustomLibraryResourceView>(moduleId, columns);
        }

        public async Task<bool> UpdateAsync(ModuleCustomLibraryInfo objModuleCustomLibraryInfo, params string[] columns)
        {
            return await _sql.UpdateAsync<ModuleCustomLibraryInfo>(objModuleCustomLibraryInfo, columns);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _sql.DeleteAsync<ModuleCustomLibraryInfo>(id);
        }
    }
}
