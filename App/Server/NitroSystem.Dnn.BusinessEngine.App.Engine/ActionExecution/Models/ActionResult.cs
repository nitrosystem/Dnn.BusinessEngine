using System;
using NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution.Enums;

namespace NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution.Models
{
    public class ActionResult
    {
        public Guid Id { get; set; }
        public ActionResultStatus Status { get; set; }
        public bool IsRedirectable { get; set; }
        public string RedirectUrl { get; set; }
        public object Data { get; set; }
    }
}
