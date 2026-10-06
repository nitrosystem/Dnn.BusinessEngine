using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.AppModel;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.AppModel
{
    public interface IAppModelRepository
    {
        Task<Guid> AddAsync(AppModelInfo objAppModelInfo);

        Task<AppModelInfo> GetAsync(Guid id);
        Task<IReadOnlyList<AppModelInfo>> GetsAsync(Guid scenarioId, params string[] columns);
        Task<(IReadOnlyList<AppModelInfo> Models, IReadOnlyList<AppModelPropertyInfo> Properties, int? TotalCount)> GetsAsync(
            Guid scenarioId,
            int pageIndex,
            int pageSize,
            string searchText,
            AppModelType? modelType,
            string sortBy);
        Task<IReadOnlyList<AppModelPropertyInfo>> GetsAsModuleVariables(Guid moduleId);

        Task UpdateGroupAsync(Guid? groupId, Guid entityId);
        Task<bool> UpdateAsync(AppModelInfo objAppModelInfo, params string[] columns);

        Task<bool> DeleteAsync(Guid id);
    }
}
