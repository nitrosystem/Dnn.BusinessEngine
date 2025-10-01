using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.DatabaseEntities.Tables
{
    [Table("BusinessEngineBasicExtensions_DataRowServices")]
    [Cacheable("BEBX_DataRowServices_", CacheItemPriority.Default, 20)]
    public class DataRowServiceInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid ServiceId { get; set; }
        public Guid AppModelId { get; set; }
        public string StoredProcedureName { get; set; }
        public string BaseQuery { get; set; }
        public string Entities { get; set; }
        public string JoinRelationships { get; set; }
        public string ModelProperties { get; set; }
        public string Filters { get; set; }
        public string Settings { get; set; }
    }
}
