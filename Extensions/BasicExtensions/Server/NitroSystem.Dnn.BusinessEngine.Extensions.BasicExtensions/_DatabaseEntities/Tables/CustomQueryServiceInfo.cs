using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Attributes;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.DatabaseEntities.Tables
{
    [Table("BusinessEngineBasicExtensions_CustomQueryServices")]
    [Cacheable("BEBX_CustomQueryServices_", CacheItemPriority.Default, 20)]
    public class CustomQueryServiceInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid? ServiceId { get; set; }
        public Guid? AppModelId { get; set; }
        public int ResultType { get; set; }
        public string StoredProcedureName { get; set; }
        public string Settings { get; set; }
    }
}
