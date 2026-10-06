using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Entity;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ListItems;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts
{
    public interface IEntityService
    {
        Task<EntityViewModel> GetEntityViewModelAsync(Guid entityId);
        Task<IEnumerable<EntityListItem>> GetEntitiesListItemAsync(Guid scenarioId, string sortBy = "ViewOrder");
        Task<(IEnumerable<EntityViewModel> Items, int? TotalCount)> GetEntitiesViewModelAsync(Guid scenarioId,
            int pageIndex,
            int pageSize,
            string searchText,
            int? entityType,
            bool? isReadonly,
            string sortBy);

        Task<Guid> CreateEntityAsync(EntityViewModel entity);

        Task UpdateEntityGroupAsync(Guid? groupId, Guid entityId);

        Task<bool> DeleteEntityAsync(Guid entityId);
    }
}
