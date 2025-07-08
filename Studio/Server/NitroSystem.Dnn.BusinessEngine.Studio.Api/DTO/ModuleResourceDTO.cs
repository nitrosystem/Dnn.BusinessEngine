using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Api.DTO
{
    public class ModuleResourceDTO
    {
        public Guid ModuleId { get; set; }
        public Guid? ParentID { get; set; }
        //public List<PageResourceInfo> Resources { get; set; }
    }
}
