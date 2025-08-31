using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.DatabaseEntities.Tables
{
    [Table("BusinessEngineBasicExtensions_CustomQueryServices")]
    [Cacheable("BEBX_CustomQueryServices_", CacheItemPriority.Default, 20)]
    public class CustomQueryServiceInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid ServiceId { get; set; }
        public string StoredProcedureName { get; set; }
        public string Settings { get; set; }
    }
}
