using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Library;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Library;

namespace NitroSystem.Dnn.BusinessEngine.Repository.Library
{
    public class LibraryResourceRepository : ILibraryResourceRepository
    {
        private readonly ISql _sql;

        public LibraryResourceRepository(ISql sql)
        {
            _sql = sql;
        }

        public async Task<Guid> AddAsync(LibraryResourceInfo objLibraryResourceInfo)
        {
            return await _sql.InsertAsync<LibraryResourceInfo>(objLibraryResourceInfo);
        }

        public async Task<LibraryResourceInfo> GetAsync(Guid id)
        {
            return await _sql.GetAsync<LibraryResourceInfo>(id);
        }

        public async Task<IReadOnlyList<LibraryResourceInfo>> GetsAsync(Guid libraryId, params string[] columns)
        {
            return await _sql.GetByScopeAsync<LibraryResourceInfo>(libraryId, columns);
        }

        public async Task<IReadOnlyList<LibraryResourceInfo>> GetsAsync(params string[] columns)
        {
            return await _sql.GetAllAsync<LibraryResourceInfo>(columns);
        }

        public async Task<bool> UpdateAsync(LibraryResourceInfo objLibraryResourceInfo, params string[] columns)
        {
            return await _sql.UpdateAsync<LibraryResourceInfo>(objLibraryResourceInfo, columns);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _sql.DeleteAsync<LibraryResourceInfo>(id);
        }
    }
}
