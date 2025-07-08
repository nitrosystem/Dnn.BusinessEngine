using NitroSystem.Dnn.BusinessEngine.Core.Contract;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views
{
    [Table("BusinessEngineView_DashboardPages")]
    [Cacheable("BE_DashboardPages_View_", CacheItemPriority.Default, 20)]
    [Scope("ScenarioId")]
    public class DashboardPageView : IEntity
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public Guid DashboardId { get; set; }
        public Guid DashboardModuleId { get; set; }
        public Guid ScenarioId { get; set; }
        public Guid? ExistingPageId { get; set; }
        public Guid? PageModuleId { get; set; }
        public Guid? ModuleId { get; set; }
        public int PageType { get; set; }
        public string PageName { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public bool IsVisible { get; set; }
        public bool InheritPermissionFromDashboard { get; set; }
        public string AuthorizationViewPage { get; set; }
        public string Settings { get; set; }
        public string ModuleType { get; set; }
        public string ModuleBuilderType { get; set; }
        public string ModuleName { get; set; }
        public string ModuleTitle { get; set; }
    }
}
