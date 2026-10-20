using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.Web.Dto
{
   public class DashboardPageModuleDto
    {
        public Guid ModuleId { get; set; }
        public string ModuleName { get; set; }
        public string PageTitle { get; set; }
        public string PageIcon { get; set; }
        public string PageDescription { get; set; }
    }
}
