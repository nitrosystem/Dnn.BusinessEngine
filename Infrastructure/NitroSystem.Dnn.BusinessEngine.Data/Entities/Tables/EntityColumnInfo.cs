using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables
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