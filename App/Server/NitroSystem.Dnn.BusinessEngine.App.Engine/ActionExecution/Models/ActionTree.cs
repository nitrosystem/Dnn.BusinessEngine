using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Dto;

namespace NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution.Models
{
    public class ActionTree
    {
        public ActionDto Action { get; set; }
        public Queue<ActionTree> CompletedActions { get; set; }
        public Queue<ActionTree> SuccessActions { get; set; }
        public Queue<ActionTree> ErrorActions { get; set; }
    }
}
