using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Action;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Action;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Procedures.Action;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Dto;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;

namespace NitroSystem.Dnn.BusinessEngine.App.ApplicationService.Action
{
    public class ActionService : IActionService
    {
        private readonly IActionRepository _actionRepository;

        public ActionService(IActionRepository actionRepository)
        {
            _actionRepository = actionRepository;
        }

        public async Task<List<ActionDto>> GetActionsAsync(
            Guid moduleId,
            Guid? fieldId = null,
            Guid? actionId = null,
            string eventName = null,
            ModuleEventTriggerOn? triggerOn = ModuleEventTriggerOn.PageLoad)
        {
            var results = await _actionRepository.GetsAsync(moduleId, fieldId, actionId, eventName, triggerOn);
            var result = HybridMapper.MapWithChildren<ActionSpResult, ActionDto, ActionParamInfo, ActionParamDto>(
              parents: results.Actions,
              children: results.Params,
              parentKeySelector: p => p.Id,
              childKeySelector: c => c.ActionId,
              assignChildren: (parent, childs) => parent.Params = childs
            );

            return result.ToList();
        }

        public async Task<IEnumerable<ActionDto>> GetActionsDtoForClientAsync(Guid moduleId)
        {
            var actions = await _actionRepository.GetsForClientAsync(moduleId);
            return HybridMapper.MapCollection<ActionForClientSpResult, ActionDto>(actions);
        }

        public async Task<string> GetBusinessControllerClassAsync(string actionType)
        {
            return await _actionRepository.GetBusinessControllerClassAsync(actionType);
        }
    }
}
