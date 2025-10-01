using System;
using NitroSystem.Dnn.BusinessEngine.Framework.Contracts;
using NitroSystem.Dnn.BusinessEngine.Framework.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Framework.Models
{
    public class ActionResult : IActionResult
    {
        public object Data { get; set; }
        public ActionResultStatus ResultStatus { get; set; }
        public Exception ErrorException { get; set; }
    }
}
