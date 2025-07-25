using DotNetNuke.Data;
using NitroSystem.Dnn.BusinessEngine.Common.Reflection;
using NitroSystem.Dnn.BusinessEngine.Core.Cashing;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Core.Providers;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;
using NitroSystem.Dnn.BusinessEngine.App.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using NitroSystem.Dnn.BusinessEngine.App.Services.Dto.Module;
using NitroSystem.Dnn.BusinessEngine.App.Services.Dto.Action;

namespace NitroSystem.Dnn.BusinessEngine.App.Services.Contracts
{
    public interface IActionService
    {
        Task<IEnumerable<ActionDto>> GetActionsDtoAsync(Guid moduleId);

        Task<IEnumerable<ActionDto>> GetActionsDtoAsync(Guid moduleId, Guid? fieldId, string eventName, bool isServerSide);

        //Task<ActionViewModel> GetActionViewModelAsync(Guid actionId);

        //Task<IEnumerable<ModuleFieldLiteDto>> GetFieldsHaveActionsAsync(Guid moduleId);
    }
}
