using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.AppModel;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.AppModel
{
    public interface IAppModelPropertyRepository
    {
        Task<Guid> AddAsync(AppModelPropertyInfo objAppModelPropertyInfo);
        Task BulkInsertAsync(IEnumerable<AppModelPropertyInfo> properties);

        Task<AppModelPropertyInfo> GetAsync(Guid id);
        Task<IReadOnlyList<AppModelPropertyInfo>> GetsAsync(Guid appModelId, params string[] columns);
        Task<IReadOnlyList<AppModelPropertyInfo>> GetsAsync(params string[] columns);

        Task<bool> UpdateAsync(AppModelPropertyInfo objAppModelPropertyInfo, params string[] columns);

        Task<bool> DeleteAsync(Guid id);
        Task<bool> DeletesAsync(Guid appModelId);
    }
}
