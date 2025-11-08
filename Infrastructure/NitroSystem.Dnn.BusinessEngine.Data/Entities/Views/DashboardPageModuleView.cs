using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Views
{
    [Table("BusinessEngineView_DashboardPageModules")]
    [Cacheable("BE_Dashboards_Pages_Modules_View_", CacheItemPriority.Default, 20)]
    public class DashboardPageModuleView : IEntity
    {
        public Guid Id { get; set; }
        public Guid PageId { get; set; }
        public Guid? ParentId { get; set; }
        public Guid ModuleId { get; set; }
        public int Wrapper { get; set; }
        public int ModuleType { get; set; }
        public string ModuleName { get; set; }
        public string ModuleTitle { get; set; }
        public string Template { get; set; }
    }
}