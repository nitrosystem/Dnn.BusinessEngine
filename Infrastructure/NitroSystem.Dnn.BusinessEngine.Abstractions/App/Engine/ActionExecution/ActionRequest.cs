using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Dto;
using System;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution
{
    public class ActionRequest
    {
        public string ConnectionId { get; set; }
        public Guid ModuleId { get; set; }
        public int UserId { get; set; }
        public string PageUrl { get; set; }
        public string BasePath { get; set; }
        public IEnumerable<ActionDto> Actions { get; set; }
        public Dictionary<string, object> ExtraParams { get; set; }
    }
}
