using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Dto
{
   public class DashboardDto
    {
        public DtoType Type => DtoType.MultipleEntities;

        public Guid Id { get; set; }
        public Guid ScenarioId { get; set; }
        public Guid ModuleId { get; set; }
        public DashboardType DashboardType { get; set; }
        public string UniqueName { get; set; }
        public string ModuleName { get; set; }
        public string ModuleTitle { get; set; }
        public int DnnModuleId { get; set; }
        public IEnumerable<string> AuthorizationViewDashboard { get; set; }
    }
}
