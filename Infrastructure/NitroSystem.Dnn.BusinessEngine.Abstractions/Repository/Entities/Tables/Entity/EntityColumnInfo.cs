using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Entity
{
    [Table("BusinessEngine_EntityColumns")]
    [Cacheable("BE_Entities_Columns_", CacheItemPriority.Default, 20)]
    [Scope("EntityId")]
    public class EntityColumnInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid EntityId { get; set; }
        public string ColumnName { get; set; }
        public string ColumnType { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsIdentity { get; set; }
        public bool AllowNulls { get; set; }
        public int ViewOrder { get; set; }
    }
}