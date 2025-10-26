using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Views
{
    [Table("BusinessEngineView_Dashboards")]
    [Cacheable("BE_Dashboards_View_", CacheItemPriority.Default, 20)]
    [Scope("ScenarioId")]
    public class DashboardView : IEntity
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public Guid ScenarioId { get; set; }
        public int PortalId { get; set; }
        public int DnnModuleId { get; set; }
        public string AuthorizationViewDashboard { get; set; }
        public string ModuleName { get; set; }
        public string ModuleTitle { get; set; }
        public string SkinName { get; set; }
        public string SkinPath { get; set; }
        public string Template { get; set; }
        public string Theme { get; set; }
        public object Settings { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public int ViewOrder { get; set; }
    }
}