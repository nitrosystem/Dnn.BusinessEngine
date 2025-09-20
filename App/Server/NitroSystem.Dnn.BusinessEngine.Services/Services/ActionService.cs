using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Tokens;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Common.IO;
using NitroSystem.Dnn.BusinessEngine.Common.Reflection;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Cashing;
using NitroSystem.Dnn.BusinessEngine.Core.Security;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using NitroSystem.Dnn.BusinessEngine.Core.Mapper;
using System.Collections.Concurrent;
using System.Threading;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.App.Services.Contracts;
using System.Globalization;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using NitroSystem.Dnn.BusinessEngine.Common.Models.Shared;
using NitroSystem.Dnn.BusinessEngine.App.Services.Dto;

namespace NitroSystem.Dnn.BusinessEngine.App.Services.Services
{
    public class ActionService : IActionService
    {
        private readonly IRepositoryBase _repository;

        public ActionService(IRepositoryBase repository)
        {
            _repository = repository;
        }

        #region Action Services

        public async Task<IEnumerable<ActionDto>> GetActionsDtoAsync(Guid moduleId, Guid? fieldId, bool executeInClientSide)
        {
            var actions = await _repository.GetByScopeAsync<ActionInfo>(moduleId, "ViewOrder");

            return actions.Where(a => a.FieldId == fieldId && a.ExecuteInClientSide == executeInClientSide).Select(action =>
            {
                return HybridMapper.Map<ActionInfo, ActionDto>(action);
            });
        }

        public IEnumerable<ActionDto> GetActionsDto(Guid moduleId, Guid? fieldId, bool executeInClientSide)
        {
            var actions = _repository.GetByScope<ActionInfo>(moduleId, "ViewOrder");

            return actions.Where(a => a.FieldId == fieldId && a.ExecuteInClientSide == executeInClientSide).Select(action =>
            {
                return HybridMapper.Map<ActionInfo, ActionDto>(action);
            });
        }

        public async Task<IEnumerable<ActionDto>> GetActionsDtoForClientAsync(Guid moduleId, Guid? fieldId = null)
        {
            var results = await _repository.ExecuteStoredProcedureMultiGridResultAsync(
                "BusinessEngine_App_GetActionsForClient", "App_ClientActions_",
                    new
                    {
                        ModuleId = moduleId,
                        FieldId = fieldId
                    },
                    grid => grid.Read<ActionInfo>(),
                    grid => grid.Read<ActionParamInfo>(),
                    grid => grid.Read<ActionResultInfo>()
                );

            var actions = results[0] as IEnumerable<ActionInfo>;
            var actionParams = results[1] as IEnumerable<ActionParamInfo>;
            var actionResults = results[2] as IEnumerable<ActionResultInfo>;

            return actions.Select(action =>
            {
                return HybridMapper.MapWithConfig<ActionInfo, ActionDto>(action,
                    (src, dest) =>
                    {
                        dest.ParentActionTriggerCondition = (ActionExecutionCondition?)action.ParentActionTriggerCondition;
                        dest.Settings = TypeCasting.TryJsonCasting<IDictionary<string, object>>(src.Settings);
                        dest.Params = actionParams.Where(p => p.ActionId == action.Id).Select(param =>
                            HybridMapper.MapWithConfig<ActionParamInfo, ParamInfo>(param,
                            (s, d) => { d.ParamValue = param.ParamValue; }));
                        dest.Results = actionResults.Where(r => r.ActionId == action.Id).Select(result =>
                        {
                            return HybridMapper.Map<ActionResultInfo, ActionResultDto>(result);
                        });
                    });
            });
        }

        public IEnumerable<ActionDto> GetActionsDtoForClient(Guid moduleId, Guid? fieldId = null)
        {
            var results = _repository.ExecuteStoredProcedureMultiGridResult(
                "BusinessEngine_App_GetActionsForClient", "App_ClientActions_",
                    new
                    {
                        ModuleId = moduleId,
                        FieldId = fieldId
                    },
                    grid => grid.Read<ActionInfo>(),
                    grid => grid.Read<ActionParamInfo>(),
                    grid => grid.Read<ActionResultInfo>()
                );

            var actions = results[0] as IEnumerable<ActionInfo>;
            var actionParams = results[1] as IEnumerable<ActionParamInfo>;
            var actionResults = results[2] as IEnumerable<ActionResultInfo>;

            return actions.Select(action =>
            {
                return HybridMapper.MapWithConfig<ActionInfo, ActionDto>(action,
                    (src, dest) =>
                    {
                        dest.ParentActionTriggerCondition = (ActionExecutionCondition?)action.ParentActionTriggerCondition;
                        dest.Settings = TypeCasting.TryJsonCasting<IDictionary<string, object>>(src.Settings);
                        dest.Params = actionParams.Where(p => p.ActionId == action.Id).Select(param =>
                            HybridMapper.MapWithConfig<ActionParamInfo, ParamInfo>(param,
                            (s, d) => { d.ParamValue = param.ParamValue; }));
                        dest.Results = actionResults.Where(r => r.ActionId == action.Id).Select(result =>
                        {
                            return HybridMapper.Map<ActionResultInfo, ActionResultDto>(result);
                        });
                    });
            });
        }

        public async Task<IEnumerable<ActionDto>> GetActionsDtoForServerAsync(IEnumerable<Guid> actionIds)
        {
            var results = await _repository.ExecuteStoredProcedureMultiGridResultAsync(
                "BusinessEngine_App_GetActionsForServer", "App_ServerActions_",
                    new
                    {
                        ActionIds = JsonConvert.SerializeObject(actionIds)
                    },
                    grid => grid.Read<ActionInfo>(),
                    grid => grid.Read<ActionParamInfo>(),
                    grid => grid.Read<ActionResultInfo>()
                );

            var actions = results[0] as IEnumerable<ActionInfo>;
            var actionParams = results[1] as IEnumerable<ActionParamInfo>;
            var actionResults = results[2] as IEnumerable<ActionResultInfo>;

            return actions.Select(action =>
            {
                return HybridMapper.MapWithConfig<ActionInfo, ActionDto>(action,
                    (src, dest) =>
                    {
                        dest.ParentActionTriggerCondition = (ActionExecutionCondition?)action.ParentActionTriggerCondition;
                        dest.Params = actionParams.Where(p => p.ActionId == action.Id).Select(param =>
                            HybridMapper.MapWithConfig<ActionParamInfo, ParamInfo>(param,
                            (s, d) => { d.ParamValue = param.ParamValue; }));
                        dest.Results = actionResults.Where(r => r.ActionId == action.Id).Select(result =>
                        {
                            return HybridMapper.Map<ActionResultInfo, ActionResultDto>(result);
                        });
                    });
            });
        }

        public IEnumerable<ActionDto> GetActionsDtoForServer(IEnumerable<Guid> actionIds)
        {
            var results = _repository.ExecuteStoredProcedureMultiGridResult(
                "BusinessEngine_App_GetActionsForServer", "App_ServerActions_",
                    new
                    {
                        ActionIds = JsonConvert.SerializeObject(actionIds)
                    },
                    grid => grid.Read<ActionInfo>(),
                    grid => grid.Read<ActionParamInfo>(),
                    grid => grid.Read<ActionResultInfo>()
                );

            var actions = results[0] as IEnumerable<ActionInfo>;
            var actionParams = results[1] as IEnumerable<ActionParamInfo>;
            var actionResults = results[2] as IEnumerable<ActionResultInfo>;

            return actions.Select(action =>
            {
                return HybridMapper.MapWithConfig<ActionInfo, ActionDto>(action,
                    (src, dest) =>
                    {
                        dest.ParentActionTriggerCondition = (ActionExecutionCondition?)action.ParentActionTriggerCondition;
                        dest.Params = actionParams.Where(p => p.ActionId == action.Id).Select(param =>
                            HybridMapper.MapWithConfig<ActionParamInfo, ParamInfo>(param,
                            (s, d) => { d.ParamValue = param.ParamValue; }));
                        dest.Results = actionResults.Where(r => r.ActionId == action.Id).Select(result =>
                        {
                            return HybridMapper.Map<ActionResultInfo, ActionResultDto>(result);
                        });
                    });
            });
        }

        public async Task<string> GetBusinessControllerClassAsync(string actionType)
        {
            return await _repository.GetColumnValueAsync<ActionTypeInfo, string>("BusinessControllerClass", "ActionType", actionType);
        }

        public string GetBusinessControllerClass(string actionType)
        {
            return _repository.GetColumnValue<ActionTypeInfo, string>("BusinessControllerClass", "ActionType", actionType);
        }

        #endregion
    }
}
