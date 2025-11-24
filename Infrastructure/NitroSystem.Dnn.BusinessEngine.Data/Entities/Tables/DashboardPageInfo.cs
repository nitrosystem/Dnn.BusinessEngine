using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables
{
    [Table("BusinessEngine_DashboardPages")]
    [Cacheable("BE_Dashboards_Pages_", CacheItemPriority.Default, 20)]
    [Scope("DashboardId")]
    public class DashboardPageInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid DashboardId { get; set; }
        public Guid? ParentId { get; set; }
        public Guid? ExistingPageId { get; set; }
        public int PageType { get; set; }
        public string PageName { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public bool IsVisible { get; set; }
        public bool InheritPermissionFromDashboard { get; set; }
        public string AuthorizationViewPage { get; set; }
        public bool IncludeModule { get; set; }
        public string Settings { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public int ViewOrder { get; set; }
    }
}