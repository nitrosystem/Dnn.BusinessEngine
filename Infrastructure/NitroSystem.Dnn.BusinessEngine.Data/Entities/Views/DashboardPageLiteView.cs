using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Views
{
    [Table("BusinessEngineView_DashboardPageListItems")]
    [Cacheable("BE_DashboardPages_View_ListItems_", CacheItemPriority.Default, 20)]
    [Scope("DashboardModuleId")]
    public class DashboardPageListItemView : IEntity
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public Guid DashboardId { get; set; }
        public Guid DashboardModuleId { get; set; }
        public string PageName { get; set; }
        public string Title { get; set; }
    }
}
