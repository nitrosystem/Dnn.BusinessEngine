using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Action;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Action;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Views.Action;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Procedures.Action;

namespace NitroSystem.Dnn.BusinessEngine.Repository.Action
{
    public class ActionRepository: IActionRepository
    {
        private readonly ISql _sql;

        public ActionRepository(ISql sql)
        {
            _sql = sql;
        }

        public async Task<Guid> AddAsync(ActionInfo objActionInfo)
        {
            return await _sql.InsertAsync<ActionInfo>(objActionInfo);
        }

        public async Task<string> GetBusinessControllerClassAsync(string actionType)
        {
            return await _sql.GetColumnValueAsync<ActionTypeInfo, string>("BusinessControllerClass", "ActionType", actionType);
        }

        public async Task<ActionInfo> GetAsync(Guid id)
        {
            return await _sql.GetAsync<ActionInfo>(id);
        }

        public async Task<ActionView> GetViewAsync(Guid id)
        {
            return await _sql.GetAsync<ActionView>(id);
        }

        public async Task<IReadOnlyList<ActionInfo>> GetsAsync(Guid moduleId, Guid? fieldId, params string[] columns)
        {
            return await _sql.GetItemsByColumnsAsync<ActionInfo>(
                new string[2] { "ModuleId", "FieldId" },
                new
                {
                    ModuleId = moduleId,
                    FieldId = fieldId
                }, columns);
        }

        public async Task<IReadOnlyList<ActionInfo>> GetsByFieldIdAsync(Guid fieldId)
        {
            return await _sql.GetItemsByColumnAsync<ActionInfo>("FieldId", fieldId);
        }

        public async Task<IReadOnlyList<ActionInfo>> GetsAsync(Guid moduleId, params string[] columns)
        {
            return await _sql.GetByScopeAsync<ActionInfo>(moduleId, columns);
        }

        public async Task<IReadOnlyList<ActionView>> GetViewsAsync(Guid moduleId, Guid? fieldId, Guid? parentId, string eventName, int executeOrder)
        {
            return await _sql.GetItemsByColumnsAsync<ActionView>(
               new string[5] { "ModuleId", "FieldId", "ParentId", "Event", "ExecuteOrder" },
               new
               {
                   ModuleId = moduleId,
                   FieldId = fieldId,
                   ParentId = parentId,
                   Event = eventName,
                   ExecuteOrder = executeOrder
               }
           );
        }

        public async Task<bool> UpdateAsync(ActionInfo objActionInfo, params string[] columns)
        {
            return await _sql.UpdateAsync<ActionInfo>(objActionInfo, columns);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _sql.DeleteAsync<ActionInfo>(id);
        }

        #region Stored Procedures

        public async Task<(IReadOnlyList<ActionView> Actions, IReadOnlyList<ActionParamInfo> Params, int TotalCount)> GetsAsync(
           Guid moduleId, Guid? fieldId, string searchText, string actionType)
        {
            var results = await _sql.ExecuteStoredProcedureMultipleAsync<int, ActionView, ActionParamInfo>(
                "dbo.BusinessEngine_Studio_GetActions", "BE_Actions_Studio_GetActions_",
                new
                {
                    ModuleId = moduleId,
                    FieldId = fieldId,
                    SearchText = searchText,
                    ActionType = actionType
                });

            var totalCount = results.Item1.Any()
                ? results.Item1.First()
                : 0;

            return (results.Item2, results.Item3, totalCount);
        }

        public async Task<(IReadOnlyList<ActionSpResult> Actions, IReadOnlyList<ActionParamInfo> Params)> GetsAsync(
            Guid moduleId,
            Guid? fieldId = null,
            Guid? actionId = null,
            string eventName = null,
            ModuleEventTriggerOn? triggerOn = ModuleEventTriggerOn.PageLoad)
        {
            return await _sql.ExecuteStoredProcedureMultipleAsync<ActionSpResult, ActionParamInfo>(
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
        }

        public async Task<IReadOnlyList<ActionForClientSpResult>> GetsForClientAsync(Guid moduleId)
        {
            return await _sql.ExecuteStoredProcedureAsListAsync<ActionForClientSpResult>(
                "dbo.BusinessEngine_App_GetActionsForClient", "BE_Actions_App_ForClient_",
                    new
                    {
                        ModuleId = moduleId
                    }
                );
        }

        #endregion
    }
}
