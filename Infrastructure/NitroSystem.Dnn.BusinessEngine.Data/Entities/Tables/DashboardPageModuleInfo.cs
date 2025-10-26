using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables
{
    [Table("BusinessEngine_DashboardPageModules")]
    [Cacheable("BE_DashboardPageModules_", CacheItemPriority.Default, 20)]
    [Scope("PageId")]
    public class DashboardPageModuleInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid PageId { get; set; }
        public Guid ModuleId { get; set; }
        public int ViewOrder { get; set; }
    }
}