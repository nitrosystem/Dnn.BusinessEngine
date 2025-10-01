using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables
{
    [Table("BusinessEngine_AppModelProperties")]
    [Cacheable("BE_AppModelProperties_", CacheItemPriority.Default, 20)]
    [Scope("AppModelId")]
    public class AppModelPropertyInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid AppModelId { get; set; }
        public string PropertyName { get; set; }
        public string PropertyType { get; set; }
        public int ViewOrder { get; set; }
    }
}