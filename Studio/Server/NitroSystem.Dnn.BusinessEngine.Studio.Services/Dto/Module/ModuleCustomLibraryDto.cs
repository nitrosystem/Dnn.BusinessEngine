using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Dto
{
   public class ModuleCustomLibraryDto
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public string LibraryName { get; set; }
        public string Version { get; set; }
        public int LoadOrder { get; set; }
    }
}
