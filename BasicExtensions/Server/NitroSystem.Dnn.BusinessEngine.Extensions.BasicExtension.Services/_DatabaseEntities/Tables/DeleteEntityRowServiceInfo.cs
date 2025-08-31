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
    [Table("BusinessEngineBasicExtensions_DeleteEntityRowServices")]
    [Cacheable("BEBX_DeleteEntityRowServices_", CacheItemPriority.Default, 20)]
    public class DeleteEntityRowServiceInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid ServiceId { get; set; }
        public Guid EntityId { get; set; }
        public string EntityTableName { get; set; }
        public string StoredProcedureName { get; set; }
        public string BaseQuery { get; set; }
        public string Conditions { get; set; }
        public string Settings { get; set; }
    }
}
