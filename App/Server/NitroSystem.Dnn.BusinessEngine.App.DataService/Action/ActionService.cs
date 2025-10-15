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
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.Dto;

namespace NitroSystem.Dnn.BusinessEngine.App.DataService.Action
{
    public class ActionService : IActionService
    {
        private readonly IRepositoryBase _repository;

        public ActionService(IRepositoryBase repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<ActionDto>> GetActionsDtoAsync(Guid moduleId, Guid? fieldId, bool executeInClientSide)
        {
            var actions = await _repository.GetByScopeAsync<ActionInfo>(moduleId, "ViewOrder");

            return actions.Where(a => a.FieldId == fieldId && a.ExecuteInClientSide == executeInClientSide).Select(action =>
            {
                return HybridMapper.Map<ActionInfo, ActionDto>(action);
            });
        }

        public async Task<IEnumerable<ActionDto>> GetActionsDtoForClientAsync(Guid moduleId)
        {
            var results = await _repository.ExecuteStoredProcedureMultipleAsync<ActionInfo, ActionParamInfo, ActionResultInfo>(
                "BusinessEngine_App_GetActionsForClient", "BE_Actions_ForClient_" + moduleId,
                    new
                    {
                        ModuleId = moduleId,
                        FieldId = (Guid?)null
                    }
                );

            var actions = results.Item1;
            var actionParams = results.Item2;
            var actionResults = results.Item3;

            var builder = new CollectionMappingBuilder<ActionInfo, ActionDto>();

            builder.AddChildAsync<ActionParamInfo, ActionParamDto, Guid>(
               source: actionParams,
               parentKey: parent => parent.Id,
               childKey: child => child.ActionId,
               assign: (dest, children) => dest.Params = children
            );

            builder.AddChildAsync<ActionResultInfo, ActionResultDto, Guid>(
              source: actionResults,
              parentKey: parent => parent.Id,
              childKey: child => child.ActionId,
              assign: (dest, children) => dest.Results = children
            );

            var result = await builder.BuildAsync(actions);
            return result;
        }

        public async Task<IEnumerable<ActionDto>> GetActionsDtoForServerAsync(IEnumerable<Guid> actionIds)
        {
            var results = await _repository.ExecuteStoredProcedureMultipleAsync<ActionInfo, ActionParamInfo, ActionResultInfo>(
                "BusinessEngine_App_GetActionsForServer", "BE_Actions_ForServer_" + string.Join("_", actionIds),
                    new
                    {
                        ActionIds = JsonConvert.SerializeObject(actionIds)
                    }
                );

            var actions = results.Item1;
            var actionParams = results.Item2;
            var actionResults = results.Item3;

            var builder = new CollectionMappingBuilder<ActionInfo, ActionDto>();

            builder.AddChildAsync<ActionParamInfo, ActionParamDto, Guid>(
               source: actionParams,
               parentKey: parent => parent.Id,
               childKey: child => child.ActionId,
               assign: (dest, children) => dest.Params = children
            );

            builder.AddChildAsync<ActionResultInfo, ActionResultDto, Guid>(
              source: actionResults,
              parentKey: parent => parent.Id,
              childKey: child => child.ActionId,
              assign: (dest, children) => dest.Results = children
            );

            var result = await builder.BuildAsync(actions);
            return result;
        }

        public async Task<string> GetBusinessControllerClass(string actionType)
        {
            return await _repository.GetColumnValueAsync<ActionTypeInfo, string>("BusinessControllerClass", "ActionType", actionType);
        }
    }
}
