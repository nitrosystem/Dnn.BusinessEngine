using NitroSystem.Dnn.BusinessEngine.Core.Contract;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Dto
{
    public class DashboardPageDto
    {
        public DtoType Type => DtoType.ServiceCalculation;

        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public Guid DashboardId { get; set; }
        public Guid DashboardModuleId { get; set; }
        public Guid ScenarioId { get; set; }
        public Guid? ExistingPageId { get; set; }
        public Guid? PageModuleId { get; set; }
        public DashboardPageType PageType { get; set; }
        public string PageName { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public bool IsVisible { get; set; }
        public bool IsChild { get; set; }
        public bool InheritPermissionFromDashboard { get; set; }
        public int ViewOrder { get; set; }
        public IEnumerable<string> AuthorizationViewPage { get; set; }
        public IDictionary<string, object> Settings { get; set; }
        public DashboardPageModuleDto Module { get; set; }
        public IEnumerable<DashboardPageDto> Pages { get; set; }
    }
}
