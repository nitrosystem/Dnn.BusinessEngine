using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables
{
    [Table("BusinessEngine_Groups")]
    [Cacheable("BE_Groups_", CacheItemPriority.Default, 20)]
    [Scope("ScenarioId")]
    public class GroupInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid? ScenarioId { get; set; }
        public string GroupType { get; set; }
        public string ObjectType { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }
        public bool IsSystemGroup { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public int ViewOrder { get; set; }
    }
}