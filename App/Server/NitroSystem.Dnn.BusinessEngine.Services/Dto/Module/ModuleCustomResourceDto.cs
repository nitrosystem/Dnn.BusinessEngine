using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.App.Services.Dto
{
   public class ModuleCustomResourceDto
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public string AddressType { get; set; }
        public string ResourceType { get; set; }
        public string ResourcePath { get; set; }
        public int LoadOrder { get; set; }
    }
}
