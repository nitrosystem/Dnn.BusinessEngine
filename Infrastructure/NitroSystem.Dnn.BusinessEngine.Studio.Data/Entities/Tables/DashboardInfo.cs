using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

using NitroSystem.Dnn.BusinessEngine.Studio.Data.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contract;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables
{
    [Table("BusinessEngine_Dashboards")]
    [Cacheable("BE_Dashboards_", CacheItemPriority.Default, 20)]
    public class DashboardInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public Guid? SkinId { get; set; }
        public int DashboardType { get; set; }
        public string UniqueName { get; set; }
        public string AuthorizationViewDashboard { get; set; }
    }
}