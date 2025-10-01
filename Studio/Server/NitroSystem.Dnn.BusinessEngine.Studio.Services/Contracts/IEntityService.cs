using System;
using System.Web;
using System.Collections.Generic;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Entity;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts
{
    public interface IEntityService
    {
        Task<EntityViewModel> GetEntityViewModelAsync(Guid entityId);
        Task<(IEnumerable<EntityViewModel> Items, int? TotalCount)> GetEntitiesViewModelAsync(Guid scenarioId, int pageIndex, int pageSize, string searchText, byte? entityType, bool? isReadonly, string sortBy);
        Task<Guid> SaveEntity(EntityViewModel entity, bool isNew, HttpContext context);
        Task<bool> UpdateGroupColumn(Guid entityId, Guid? groupId);
        Task<bool> DeleteEntityAsync(Guid entityId);
    }
}
