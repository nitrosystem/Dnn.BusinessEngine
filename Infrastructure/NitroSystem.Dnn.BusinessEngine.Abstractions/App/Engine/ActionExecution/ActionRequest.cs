using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Dto;
using System;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution
{
    public class ActionDto
    {
        
        public string BasePath { get; set; }
        public IEnumerable<ActionDto> Actions { get; set; }
    }
}
