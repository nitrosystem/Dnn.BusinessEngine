using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Api.DTO
{
    public class CheckModuleNameDTO
    {
        public Guid ScenarioID{ get; set; }
        public Guid? ModuleID { get; set; }
        public string ModuleName { get; set; }
    }
}
