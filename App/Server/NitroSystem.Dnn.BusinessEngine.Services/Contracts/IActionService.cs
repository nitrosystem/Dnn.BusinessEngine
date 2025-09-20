using DotNetNuke.Data;
using NitroSystem.Dnn.BusinessEngine.Common.Reflection;
using NitroSystem.Dnn.BusinessEngine.Core.Cashing;
using NitroSystem.Dnn.BusinessEngine.Core.Security;
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
using NitroSystem.Dnn.BusinessEngine.App.Services.Dto;

namespace NitroSystem.Dnn.BusinessEngine.App.Services.Contracts
{
    public interface IActionService
    {
        Task<IEnumerable<ActionDto>> GetActionsDtoAsync(Guid moduleId, Guid? fieldId, bool executeInClientSide);
        IEnumerable<ActionDto> GetActionsDto(Guid moduleId, Guid? fieldId, bool executeInClientSide);

        Task<IEnumerable<ActionDto>> GetActionsDtoForClientAsync(Guid moduleId, Guid? fieldId = null);
        IEnumerable<ActionDto> GetActionsDtoForClient(Guid moduleId, Guid? fieldId = null);

        Task<IEnumerable<ActionDto>> GetActionsDtoForServerAsync(IEnumerable<Guid> actionIds);
        IEnumerable<ActionDto> GetActionsDtoForServer(IEnumerable<Guid> actionIds);

        Task<string> GetBusinessControllerClassAsync(string actionType);
        string GetBusinessControllerClass(string actionType);

        //Task<ActionViewModel> GetActionViewModelAsync(Guid actionId);

        //Task<IEnumerable<ModuleFieldLiteDto>> GetFieldsHaveActionsAsync(Guid moduleId);
    }
}
