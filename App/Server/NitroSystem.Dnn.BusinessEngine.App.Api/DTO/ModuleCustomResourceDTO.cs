using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Api.DTO
{
    public class ModuleCustomResourceDTO
    {
        public Guid ModuleID { get; set; }
        public Guid ItemID { get; set; }
        public int LoadOrder { get; set; }
    }
}
