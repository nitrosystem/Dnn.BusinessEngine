using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Dto;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Contracts
{
    public interface IActionService
    {
        Task<List<ActionDto>> GetActionsAsync(Guid moduleId,
            Guid? fieldId = null,
            Guid? actionId = null,
            string eventName = null,
            ModuleEventTriggerOn? triggerOn = null);
        Task<IEnumerable<ActionDto>> GetActionsDtoForClientAsync(Guid moduleId);
        Task<string> GetBusinessControllerClassAsync(string actionType);
    }
}
