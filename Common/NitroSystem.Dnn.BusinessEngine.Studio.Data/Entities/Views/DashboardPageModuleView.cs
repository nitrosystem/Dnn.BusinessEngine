using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contract;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views
{
    [Table("BusinessEngineView_DashboardPageModules")]
    [Cacheable("BE_DashboardPageModules_View_", CacheItemPriority.Default, 20)]
    [Scope("DashboardModuleId")]
    public class DashboardPageModuleView : IEntity
    {
        public Guid Id { get; set; }
        public Guid DashboardModuleId { get; set; }
        public Guid PageId { get; set; }
        public Guid ModuleId { get; set; }
        public Guid? ParentId { get; set; }
        public string PageName { get; set; }
        public string ModuleType { get; set; }
        public string ModuleBuilderType { get; set; }
        public string ModuleName { get; set; }
        public string ModuleTitle { get; set; }
    }
}