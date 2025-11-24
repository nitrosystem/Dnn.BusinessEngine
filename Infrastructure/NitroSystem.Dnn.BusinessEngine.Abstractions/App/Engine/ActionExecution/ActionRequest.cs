using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution
{
    public class ActionRequest
    {
        public string ConnectionId { get; set; }
        public Guid ModuleId { get; set; }
        public int UserId { get; set; }
        public string PageUrl { get; set; }
        public bool ByEvent { get; set; }
        public IEnumerable<ActionDto> Actions { get; set; }
    }
}
