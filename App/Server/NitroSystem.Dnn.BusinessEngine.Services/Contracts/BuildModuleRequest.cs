using NitroSystem.Dnn.BusinessEngine.App.Services.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.App.Services.Contracts
{
    public class BuildModuleRequest
    {
        public BuildScope Scope { get; set; }
        public Guid? ModuleId { get; set; }
        public Guid? ParentId { get; set; }
        public Guid? ScenarioId { get; set; }
    }

}
