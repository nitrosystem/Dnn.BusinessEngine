using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Dto;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.App.Engine.ActionOrchestrator
{
    public class ActionTree
    {
        public ActionDto Action { get; set; }
        public Queue<ActionTree> CompletedActions { get; set; }
        public Queue<ActionTree> SuccessActions { get; set; }
        public Queue<ActionTree> ErrorActions { get; set; }
    }
}
