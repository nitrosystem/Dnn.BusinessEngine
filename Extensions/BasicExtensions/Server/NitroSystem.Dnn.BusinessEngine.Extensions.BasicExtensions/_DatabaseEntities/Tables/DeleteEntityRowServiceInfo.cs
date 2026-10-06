using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Attributes;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.DatabaseEntities.Tables
{
    [Table("BusinessEngineBasicExtensions_DeleteEntityRowServices")]
    [Cacheable("BEBX_DeleteEntityRowServices_", CacheItemPriority.Default, 20)]
    public class DeleteEntityRowServiceInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid? ServiceId { get; set; }
        public Guid EntityId { get; set; }
        public string EntityTableName { get; set; }
        public string StoredProcedureName { get; set; }
        public string BaseQuery { get; set; }
        public string Conditions { get; set; }
        public string Settings { get; set; }
    }
}
