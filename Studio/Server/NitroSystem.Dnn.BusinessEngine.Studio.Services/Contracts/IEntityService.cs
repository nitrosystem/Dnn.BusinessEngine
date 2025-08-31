using DotNetNuke.Data;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

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
