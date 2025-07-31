using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables
{
    [Table("BusinessEngine_Services")]
    [Cacheable("BE_Services_", CacheItemPriority.Default, 20)]
    [Scope("ScenarioId")]
    public class ServiceInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid ScenarioId { get; set; }
        public Guid? GroupId { get; set; }
        public string ServiceType { get; set; }
        public string ServiceName { get; set; }
        public string Settings { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public int ViewOrder { get; set; }
    }
}