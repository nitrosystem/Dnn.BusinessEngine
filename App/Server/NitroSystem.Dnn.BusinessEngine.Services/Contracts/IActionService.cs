using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.App.Services.Dto;

namespace NitroSystem.Dnn.BusinessEngine.App.Services.Contracts
{
    public interface IActionService
    {
        Task<IEnumerable<ActionDto>> GetActionsDtoAsync(Guid moduleId, Guid? fieldId, bool executeInClientSide);
        Task<IEnumerable<ActionDto>> GetActionsDtoForClientAsync(Guid moduleId);
        Task<IEnumerable<ActionDto>> GetActionsDtoForServerAsync(IEnumerable<Guid> actionIds);
        Task<string> GetBusinessControllerClass(string actionType);
    }
}
