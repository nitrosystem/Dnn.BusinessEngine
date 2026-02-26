using NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution.Enums;
using System;

namespace NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution.Models
{
    public class ActionResult
    {
        public Guid ActionId { get; set; }
        public ActionResultStatus Status { get; set; }
        public bool IsRedirectable { get; set; }
        public string RedirectUrl { get; set; }
        public int ExecuteOrder { get; set; }
    }
}
