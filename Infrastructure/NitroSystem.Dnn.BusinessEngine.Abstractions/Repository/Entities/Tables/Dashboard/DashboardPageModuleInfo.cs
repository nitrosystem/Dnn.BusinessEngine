using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Attributes;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Dashboard
{
    [Table("BusinessEngine_DashboardPageModules")]
    [Cacheable("BE_Dashboards_Pages_Modules_", CacheItemPriority.Default, 20)]
    [Scope("PageId")]
    public class DashboardPageModuleInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid PageId { get; set; }
        public Guid ModuleId { get; set; }
    }
}