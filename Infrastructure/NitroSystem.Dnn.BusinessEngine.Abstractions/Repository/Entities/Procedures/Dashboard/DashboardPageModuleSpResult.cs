using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Procedures.Dashboard
{
   public class DashboardPageModuleSpResult
    {
        public Guid ModuleId { get; set; }
        public string ModuleName { get; set; }
        public bool IsSSR { get; set; }
        public string PageTitle { get; set; }
        public string PageIcon { get; set; }
        public string PageDescription { get; set; }
    }
}
