using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Data.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;

namespace NitroSystem.Dnn.BusinessEngine.App.DataService.Action
{
    public class ActionService : IActionService
    {
        private readonly IRepositoryBase _repository;

        public ActionService(IRepositoryBase repository)
        {
            _repository = repository;
        }

        public async Task<List<ActionDto>> GetActionsAsync(Guid moduleId, Guid? fieldId = null, Guid? actionId = null, string eventName = null, ModuleEventTriggerOn? triggerOn = null)
        {
            var results = await _repository.ExecuteStoredProcedureMultipleAsync<ActionInfo, ActionParamInfo>(
               "dbo.BusinessEngine_App_GetActions", "BE_Actions_App_",
                   new
                   {
                       ModuleId = moduleId,
                       FieldId = fieldId,
                       ActionId = actionId,
                       Event = eventName,
                       TriggerOn = triggerOn
                   }
               );

            var actions = results.Item1;
            var actionParams = results.Item2;

            var result = HybridMapper.MapWithChildren<ActionInfo, ActionDto, ActionParamInfo, ActionParamDto>(
              parents: actions,
              children: actionParams,
              parentKeySelector: p => p.Id,
              childKeySelector: c => c.ActionId,
              assignChildren: (parent, childs) => parent.Params = childs
            );

            return result.ToList();
        }

        public async Task<IEnumerable<ActionDto>> GetActionsDtoForClientAsync(Guid moduleId)
        {
            var actions = await _repository.ExecuteStoredProcedureAsListAsync<ActionInfo>(
                "dbo.BusinessEngine_App_GetActionsForClient", "BE_Actions_App_ForClient_",
                    new
                    {
                        ModuleId = moduleId
                    }
                );

            return HybridMapper.MapCollection<ActionInfo, ActionDto>(actions).ToList();
        }

        public async Task<string> GetBusinessControllerClass(string actionType)
        {
            return await _repository.GetColumnValueAsync<ActionTypeInfo, string>("BusinessControllerClass", "ActionType", actionType);
        }
    }
}
