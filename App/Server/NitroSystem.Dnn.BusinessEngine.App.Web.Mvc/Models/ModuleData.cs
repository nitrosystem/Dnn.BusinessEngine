using NitroSystem.Dnn.BusinessEngine.App.Services.Dto;
using NitroSystem.Dnn.BusinessEngine.App.Services.ViewModels.Module.Field;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.App.Web.Mvc.Models
{
    public class ModuleData
    {
        public ConcurrentDictionary<string, object> data { get; set; }
        public IEnumerable<ModuleVariableDto> variables { get; set; }
        public IEnumerable<ModuleFieldViewModel> fields { get; set; }
        public IEnumerable<ActionDto> actions { get; set; }
    }
}