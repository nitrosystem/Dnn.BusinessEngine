using System;
using System.Linq;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.Models;

namespace NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecutionEngine.Services
{
    public static class BuildBufferService
    {
        public static Queue<ActionTree> BuildBufferByEvent(List<ActionDto> actions)
        {
            foreach (var action in actions)
            {
                var lookup = actions.Where(p => p.ParentId != null)
                    .GroupBy(a => a.ParentId)
                    .ToDictionary(g => g.Key, g => g.OrderBy(x => x.ViewOrder).ToList());

                actions.AddRange(CollectActions(action, lookup, new List<ActionDto>()));
            }

            var buffer = BuildActionTree(actions);
            return buffer;
        }

        public static Queue<ActionTree> BuildBuffer(IEnumerable<ActionDto> actions)
        {
            var buffer = BuildActionTree(actions);
            return buffer;
        }

        private static Queue<ActionTree> BuildActionTree(IEnumerable<ActionDto> actions)
        {
            // ساخت دیکشنری lookup برای دسترسی سریع به ActionTreeها
            var lookup = actions.ToDictionary(a => a.Id, a => new ActionTree
            {
                Action = a,
                CompletedActions = new Queue<ActionTree>(),
                SuccessActions = new Queue<ActionTree>(),
                ErrorActions = new Queue<ActionTree>()
            });

            // نگهداری ریشه‌ها
            var roots = new Queue<ActionTree>();

            foreach (var action in actions)
            {
                if (action.ParentId == null)
                {
                    // ریشه‌ها
                    roots.Enqueue(lookup[action.Id]);
                }
                else
                {
                    var parentTree = lookup[action.ParentId.Value];
                    var childTree = lookup[action.Id];

                    switch (action.ParentActionTriggerCondition)
                    {
                        case ActionExecutionCondition.AlwaysExecute:
                            parentTree.CompletedActions.Enqueue(childTree);
                            break;

                        case ActionExecutionCondition.ExecuteOnSuccess:
                            parentTree.SuccessActions.Enqueue(childTree);
                            break;

                        case ActionExecutionCondition.ExecuteOnError:
                            parentTree.ErrorActions.Enqueue(childTree);
                            break;
                    }
                }
            }

            return roots;
        }

        private static IEnumerable<ActionDto> CollectActions(ActionDto root, Dictionary<Guid?, List<ActionDto>> lookup, List<ActionDto> result)
        {
            result.Add(root);

            if (lookup.TryGetValue(root.Id, out var children))
            {
                foreach (var child in children)
                {
                    CollectActions(child, lookup, result);
                }
            }

            return result;
        }

        private static Queue<ActionTree> GetActionChilds(IEnumerable<ActionDto> actions, Queue<ActionTree> buffer, Guid parentId, ActionExecutionCondition parentResultStatus)
        {
            foreach (var action in actions.Where(a => a.ParentId == parentId && a.ParentActionTriggerCondition == parentResultStatus) ?? Enumerable.Empty<ActionDto>())
            {
                var node = new ActionTree()
                {
                    Action = action,
                    CompletedActions = new Queue<ActionTree>(),
                    SuccessActions = new Queue<ActionTree>(),
                    ErrorActions = new Queue<ActionTree>()
                };

                GetActionChilds(actions, node.CompletedActions, action.Id, ActionExecutionCondition.AlwaysExecute);
                GetActionChilds(actions, node.SuccessActions, action.Id, ActionExecutionCondition.ExecuteOnSuccess);
                GetActionChilds(actions, node.ErrorActions, action.Id, ActionExecutionCondition.ExecuteOnError);

                buffer.Enqueue(node);
            }

            return buffer;
        }
    }
}
