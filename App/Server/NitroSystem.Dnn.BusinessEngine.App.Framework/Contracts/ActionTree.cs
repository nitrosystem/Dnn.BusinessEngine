using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.App.Services.Dto;

namespace NitroSystem.Dnn.BusinessEngine.Framework.Contracts
{
    public class ActionTree
    {
        public ActionDto Action { get; set; }
        public Queue<ActionTree> CompletedActions { get; set; }
        public Queue<ActionTree> SuccessActions { get; set; }
        public Queue<ActionTree> ErrorActions { get; set; }
    }
}
