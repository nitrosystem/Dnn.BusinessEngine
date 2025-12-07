using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Procedures
{
   public class DashboardPageModuleSpResult
    {
        public Guid ModuleId { get; set; }
        public string ModuleName { get; set; }
        public string PageTitle { get; set; }
        public string PageIcon { get; set; }
        public string PageDescription { get; set; }
    }
}
