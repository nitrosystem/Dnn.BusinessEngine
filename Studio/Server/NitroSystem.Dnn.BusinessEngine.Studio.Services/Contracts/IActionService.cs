using DotNetNuke.Data;
using NitroSystem.Dnn.BusinessEngine.Common.Reflection;
using NitroSystem.Dnn.BusinessEngine.Core.Cashing;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Core.Providers;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Dto;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Mapping;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Enums;
using NitroSystem.Dnn.BusinessEngine.Studio.ApplicationActions.Mapping;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts
{
    public interface IActionService
    {
        Task<IEnumerable<ActionTypeViewModel>> GetActionTypesViewModelAsync(string sortBy = "ViewOrder");

        Task<(IEnumerable<ActionViewModel> Items, int TotalCount)> GetActionsViewModelAsync(Guid moduleId, Guid? fieldId, int pageIndex, int pageSize, string searchText, string actionType, string sortBy);

        Task<ActionViewModel> GetActionViewModelAsync(Guid actionId);

        Task<Guid> SaveActionAsync(ActionViewModel action, bool isNew);

        Task<bool> DeleteActionAsync(Guid id);
    }
}
