using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Extension;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Extension
{
    public interface IProviderRepository
    {
        Task<Guid> AddAsync(ProviderInfo objProviderInfo);

        Task<ProviderInfo> GetAsync(Guid id);
        Task<IReadOnlyList<ProviderInfo>> GetsAsync(params string[] columns);

        Task<bool> UpdateAsync(ProviderInfo objProviderInfo, params string[] columns);

        Task<bool> DeleteAsync(Guid id);
    }
}
