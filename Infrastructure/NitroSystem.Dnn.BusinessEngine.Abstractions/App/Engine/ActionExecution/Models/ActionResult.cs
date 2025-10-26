using System;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.Models
{
    public class ActionResult : IActionResult
    {
        public object Data { get; set; }
        public ActionResultStatus Status { get; set; }
        public Exception ErrorException { get; set; }
    }
}
