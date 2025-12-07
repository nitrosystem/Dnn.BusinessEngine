using System;
using System.Linq;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.Models;

namespace NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecutionEngine.Services
{
    public class BuildBufferService : IBuildBufferService
    {
        // اگر در آینده بخواهی logger، telemetry یا config اضافه کنی، این constructor آماده است
        //private readonly ILogger<BuildBufferService>? _logger;

        //public BuildBufferService(ILogger<BuildBufferService>? logger = null)
        //{
        //    _logger = logger;
        //}

        public Queue<ActionTree> BuildBuffer(List<ActionDto> actions)
        {
            if (actions == null || actions.Count == 0)
                return new Queue<ActionTree>();

            // ساخت lookup فقط یک‌بار (ParentId -> فرزندان)
            var lookup = actions
                .Where(p => p.ParentId != null)
                .GroupBy(a => a.ParentId)
                .ToDictionary(g => g.Key, g => g.OrderBy(x => x.ViewOrder).ToList());

            // از اضافه کردن همزمان جلوگیری می‌کنیم → snapshot می‌سازیم
            var collected = new HashSet<Guid>();
            var result = new List<ActionDto>();

            foreach (var root in actions.Where(a => a.ParentId == null))
            {
                CollectActions(root, lookup, result, collected);
            }

            // در نهایت ساخت درخت اکشن‌ها
            return BuildActionTree(result);
        }

        /// <summary>
        /// درخت اکشن‌ها را از روی لیست مسطح می‌سازد
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
        /// گردآوری بازگشتی اکشن‌های فرزند
        /// </summary>
        private static void CollectActions(
            ActionDto root,
            IReadOnlyDictionary<Guid?, List<ActionDto>> lookup,
            ICollection<ActionDto> result,
            ISet<Guid> visited)
        {
            if (!visited.Add(root.Id))
                return; // جلوگیری از حلقه‌های احتمالی

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
