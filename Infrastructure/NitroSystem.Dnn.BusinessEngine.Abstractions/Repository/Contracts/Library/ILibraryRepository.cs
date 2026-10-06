using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Library;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Library
{
    public interface ILibraryRepository
    {
        Task<Guid> AddAsync(LibraryInfo objLibraryInfo);

        Task<LibraryInfo> GetAsync(Guid id);
        Task<IReadOnlyList<LibraryInfo>> GetsAsync(params string[] columns);

        Task<bool> UpdateAsync(LibraryInfo objLibraryInfo, params string[] columns);

        Task<bool> DeleteAsync(Guid id);
    }
}
