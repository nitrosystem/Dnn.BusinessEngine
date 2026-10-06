using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Attributes;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Dashboard
{
    [Table("BusinessEngine_Dashboards")]
    [Cacheable("BE_Dashboards_", CacheItemPriority.Default, 20)]
    public class DashboardInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public string AuthorizationViewDashboard { get; set; }
    }
}