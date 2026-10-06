using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Library;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Library
{
    public interface ILibraryResourceRepository
    {
        Task<Guid> AddAsync(LibraryResourceInfo objLibraryResourceInfo);

        Task<LibraryResourceInfo> GetAsync(Guid id);
        Task<IReadOnlyList<LibraryResourceInfo>> GetsAsync(Guid libraryId, params string[] columns);
        Task<IReadOnlyList<LibraryResourceInfo>> GetsAsync(params string[] columns);

        Task<bool> UpdateAsync(LibraryResourceInfo objLibraryResourceInfo, params string[] columns);

        Task<bool> DeleteAsync(Guid id);
    }
}
