using System;
using System.Web;
using System.Collections.Generic;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Entity;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ListItems;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts
{
    public interface IEntityService
    {
        Task<(IEnumerable<EntityViewModel> Items, int? TotalCount)> GetEntitiesViewModelAsync(Guid scenarioId,
            int pageIndex,
            int pageSize,
            string searchText,
            byte? entityType,
            bool? isReadonly,
            string sortBy);
        Task<IEnumerable<EntityListItem>> GetEntitiesListItemAsync(Guid scenarioId, string sortBy = "ViewOrder");
        Task<EntityViewModel> GetEntityViewModelAsync(Guid entityId);
        Task<(IEnumerable<string> Tables, IEnumerable<string> Views)> GetDatabaseObjects();
        Task<IEnumerable<TableColumnInfo>> GetDatabaseObjectColumns(string objectName);
        Task<Guid> SaveEntity(EntityViewModel entity, bool isNew, HttpContext context);
        Task<bool> UpdateGroupColumn(Guid entityId, Guid? groupId);
        Task<bool> DeleteEntityAsync(Guid entityId);
    }
}
