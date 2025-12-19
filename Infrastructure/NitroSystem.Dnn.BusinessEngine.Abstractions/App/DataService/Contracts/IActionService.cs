using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Dto;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Contracts
{
    public interface IActionService
    {
        Task<IEnumerable<Guid>> GetActionIdsAsync(Guid moduleId, Guid? fieldId = null, string eventName = null);
        Task<IEnumerable<ActionDto>> GetActionsDtoAsync(Guid moduleId, Guid? fieldId, bool executeInClientSide);
        Task<IEnumerable<ActionDto>> GetActionsDtoForClientAsync(Guid moduleId);
        Task<List<ActionDto>> GetActionsDtoForServerAsync(IEnumerable<Guid> actionIds);
        Task<string> GetBusinessControllerClass(string actionType);
    }
}
