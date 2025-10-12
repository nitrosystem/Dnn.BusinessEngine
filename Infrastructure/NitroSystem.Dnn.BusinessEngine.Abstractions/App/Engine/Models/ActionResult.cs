using System;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.Models
{
    public class ActionResult : IActionResult
    {
        public object Data { get; set; }
        public ActionResultStatus Status { get; set; }
        public Exception ErrorException { get; set; }
    }
}
