using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

using NitroSystem.Dnn.BusinessEngine.Studio.Data.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contract;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables
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