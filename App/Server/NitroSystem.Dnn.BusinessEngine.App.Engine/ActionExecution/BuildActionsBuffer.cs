using System;
using System.Linq;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Dto;
using NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution.Models;

namespace NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution
{
    public static class BuildActionsBuffer  
    {
        public static Queue<ActionTree> BuildBuffer(List<ActionDto> actions)
        {
            if (actions == null || actions.Count == 0)
                return new Queue<ActionTree>();

            // Building a lookup table once (ParentId -> Children)
            var lookup = actions
                .Where(p => p.ParentId != null)
                .GroupBy(a => a.ParentId)
                .ToDictionary(g => g.Key, g => g.OrderBy(x => x.ExecuteOrder).ToList());

            var collected = new HashSet<Guid>();
            var result = new List<ActionDto>();

            foreach (var root in actions.Where(a => a.ParentId == null))
            {
                CollectActions(root, lookup, result, collected);
            }

            // Finaly, building the actions tree
            return BuildActionTree(result);
        }

        /// <summary>
        /// create action tree from a flat list.
        /// </summary>
        private static Queue<ActionTree> BuildActionTree(IEnumerable<ActionDto> actions)
        {
            var lookup = actions.ToDictionary(
                a => a.Id,
                a => new ActionTree
                {
                    Action = a,
                    CompletedActions = new Queue<ActionTree>(),
                    SuccessActions = new Queue<ActionTree>(),
                    ErrorActions = new Queue<ActionTree>()
                });

            var roots = new Queue<ActionTree>();

            foreach (var action in actions)
            {
                if (action.ParentId == null)
                {
                    roots.Enqueue(lookup[action.Id]);
                    continue;
                }

                if (!lookup.TryGetValue(action.ParentId.Value, out var parentTree))
                    continue;

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

            return roots;
        }

        /// <summary>
        /// Recursive collection of child actions
        /// </summary>
        private static void CollectActions(
            ActionDto root,
            IReadOnlyDictionary<Guid?, List<ActionDto>> lookup,
            ICollection<ActionDto> result,
            ISet<Guid> visited)
        {
            if (!visited.Add(root.Id))
                return; //Prevention of loops

            result.Add(root);

            if (!lookup.TryGetValue(root.Id, out var children))
                return;

            foreach (var child in children)
            {
                CollectActions(child, lookup, result, visited);
            }
        }
    }
}
