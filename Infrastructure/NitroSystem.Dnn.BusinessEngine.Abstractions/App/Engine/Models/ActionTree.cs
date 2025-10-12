using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.Dto;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.Models
{
    public class ActionTree
    {
        public ActionDto Action { get; set; }
        public Queue<ActionTree> CompletedActions { get; set; }
        public Queue<ActionTree> SuccessActions { get; set; }
        public Queue<ActionTree> ErrorActions { get; set; }
    }
}
