using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Api.Dto
{
    public class CheckModuleNameDto
    {
        public Guid ScenarioId{ get; set; }
        public Guid? ModuleId { get; set; }
        public string ModuleName { get; set; }
    }
}
