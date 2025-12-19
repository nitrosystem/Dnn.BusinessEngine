using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Data.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Dto;

namespace NitroSystem.Dnn.BusinessEngine.App.DataService.Action
{
    public class ActionService : IActionService
    {
        private readonly IRepositoryBase _repository;

        public ActionService(IRepositoryBase repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Guid>> GetActionIdsAsync(Guid moduleId, Guid? fieldId = null, string eventName = null)
        {
            var actions = await _repository.GetByScopeAsync<ActionInfo>(moduleId, "ViewOrder");
            return actions
                .Where(a => a.Event == eventName && a.FieldId == fieldId)
                .Select(a => a.Id)
                .ToList();
        }

        public async Task<IEnumerable<ActionDto>> GetActionsDtoAsync(Guid moduleId, Guid? fieldId, bool executeInClientSide)
        {
            var actions = await _repository.GetByScopeAsync<ActionInfo>(moduleId, "ViewOrder");
            var finalizedActions = actions
                .Where(a => a.FieldId == fieldId && a.ExecuteInClientSide == executeInClientSide)
                .ToList();

            return HybridMapper.MapCollection<ActionInfo, ActionDto>(finalizedActions);
        }

        public async Task<IEnumerable<ActionDto>> GetActionsDtoForClientAsync(Guid moduleId)
        {
            var results = await _repository.ExecuteStoredProcedureMultipleAsync<ActionInfo, ActionParamInfo>(
                "dbo.BusinessEngine_App_GetActionsForClient", "BE_Actions_ForClient_",
                    new
                    {
                        ModuleId = moduleId,
                        FieldId = (Guid?)null
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

            return result;
        }

        public async Task<List<ActionDto>> GetActionsDtoForServerAsync(IEnumerable<Guid> actionIds)
        {
            var results = await _repository.ExecuteStoredProcedureMultipleAsync<ActionInfo, ActionParamInfo>(
                "dbo.BusinessEngine_App_GetActionsForServer", "BE_Actions_ForServer_",
                    new
                    {
                        ActionIds = JsonConvert.SerializeObject(actionIds)
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

        public async Task<string> GetBusinessControllerClass(string actionType)
        {
            return await _repository.GetColumnValueAsync<ActionTypeInfo, string>("BusinessControllerClass", "ActionType", actionType);
        }
    }
}
