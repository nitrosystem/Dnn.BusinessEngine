using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

using NitroSystem.Dnn.BusinessEngine.Studio.Data.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contract;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Repository;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables
{
    [Table("BusinessEngine_EntityColumns")]
    [Cacheable("BE_EntityColumns_", CacheItemPriority.Default, 20)]
    [Scope("EntityId")]
    public class EntityColumnInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid EntityId { get; set; }
        public string ColumnName { get; set; }
        public string ColumnType { get; set; }
        public bool AllowNulls { get; set; }
        public string DefaultValue { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsUnique { get; set; }
        public bool IsComputedColumn { get; set; }
        public bool IsIdentity { get; set; }
        public string Formula { get; set; }
        public string Settings { get; set; }
        public string Description { get; set; }
        public int ViewOrder { get; set; }
    }
}