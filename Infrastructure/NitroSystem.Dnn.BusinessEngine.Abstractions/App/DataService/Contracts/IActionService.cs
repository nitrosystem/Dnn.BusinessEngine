using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Contracts
{
    public interface IActionService
    {
        Task<List<ActionDto>> GetActionsAsync(Guid moduleId, Guid? fieldId = null, Guid? actionId = null, string eventName = null, ModuleEventTriggerOn? triggerOn = null);
        Task<IEnumerable<ActionDto>> GetActionsDtoForClientAsync(Guid moduleId);
        Task<string> GetBusinessControllerClass(string actionType);
    }
}
