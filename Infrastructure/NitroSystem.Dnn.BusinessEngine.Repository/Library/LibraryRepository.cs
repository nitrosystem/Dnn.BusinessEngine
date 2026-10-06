using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Library;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Library;

namespace NitroSystem.Dnn.BusinessEngine.Repository.Library
{
    public class LibraryRepository : ILibraryRepository
    {
        private readonly ISql _sql;

        public LibraryRepository(ISql sql)
        {
            _sql = sql;
        }

        public async Task<Guid> AddAsync(LibraryInfo objLibraryInfo)
        {
            return await _sql.InsertAsync<LibraryInfo>(objLibraryInfo);
        }

        public async Task<LibraryInfo> GetAsync(Guid id)
        {
            return await _sql.GetAsync<LibraryInfo>(id);
        }

        public async Task<IReadOnlyList<LibraryInfo>> GetsAsync(params string[] columns)
        {
            return await _sql.GetAllAsync<LibraryInfo>(columns);
        }

        public async Task<bool> UpdateAsync(LibraryInfo objLibraryInfo, params string[] columns)
        {
            return await _sql.UpdateAsync<LibraryInfo>(objLibraryInfo, columns);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _sql.DeleteAsync<LibraryInfo>(id);
        }
    }
}
