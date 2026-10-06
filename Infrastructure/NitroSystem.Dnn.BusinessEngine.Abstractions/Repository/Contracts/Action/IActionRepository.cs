using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Action;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Views.Action;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Procedures.Action;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Action
{
    public interface IActionRepository
    {
        Task<Guid> AddAsync(ActionInfo objActionInfo);

        Task<string> GetBusinessControllerClassAsync(string actionType);
        Task<ActionInfo> GetAsync(Guid id);
        Task<ActionView> GetViewAsync(Guid id);
        Task<IReadOnlyList<ActionInfo>> GetsAsync(Guid moduleId, Guid? fieldId, params string[] columns);
        Task<IReadOnlyList<ActionInfo>> GetsByFieldIdAsync(Guid fieldId);
        Task<IReadOnlyList<ActionInfo>> GetsAsync(Guid moduleId, params string[] columns);
        Task<IReadOnlyList<ActionView>> GetViewsAsync(Guid moduleId, Guid? fieldId, Guid? parentId, string eventName, int executeOrder);
        Task<(IReadOnlyList<ActionView> Actions, IReadOnlyList<ActionParamInfo> Params, int TotalCount)> GetsAsync(
           Guid moduleId, Guid? fieldId, string searchText, string actionType);
        Task<(IReadOnlyList<ActionSpResult> Actions, IReadOnlyList<ActionParamInfo> Params)> GetsAsync(
            Guid moduleId,
            Guid? fieldId = null,
            Guid? actionId = null,
            string eventName = null,
            ModuleEventTriggerOn? triggerOn = ModuleEventTriggerOn.PageLoad);
        Task<IReadOnlyList<ActionForClientSpResult>> GetsForClientAsync(Guid moduleId);

        Task<bool> UpdateAsync(ActionInfo objActionInfo, params string[] columns);

        Task<bool> DeleteAsync(Guid id);
    }
}
