using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.DB.Entities
{
    [Table("BusinessEngineBasicExtensions_BindEntityServices")]
    [Cacheable("BEBX_BindEntityServices_", CacheItemPriority.Default, 20)]
    public class BindEntityServiceInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid ServiceId { get; set; }
        public Guid EntityId { get; set; }
        public Guid AppModelId { get; set; }
        public string EntityTableName { get; set; }
        public string StoredProcedureName { get; set; }
        public string BaseQuery { get; set; }
        public string ModelProperties { get; set; }
        public string Filters { get; set; }
        public string Settings { get; set; }
    }
}
