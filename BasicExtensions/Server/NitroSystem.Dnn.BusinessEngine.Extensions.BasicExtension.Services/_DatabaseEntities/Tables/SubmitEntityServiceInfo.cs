using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.DatabaseEntities.Tables
{
    [Table("BusinessEngineBasicExtensions_SubmitEntityServices")]
    [Cacheable("BEBX_SubmitEntityServices_", CacheItemPriority.Default, 20)]
    public class SubmitEntityServiceInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid ServiceId { get; set; }
        public Guid EntityId { get; set; }
        public string BaseQuery { get; set; }
        public string InsertBaseQuery { get; set; }
        public string UpdateBaseQuery { get; set; }
        public string StoredProcedureName { get; set; }
        public int ActionType { get; set; }
        public string Entity { get; set; }
        public string Settings { get; set; }
    }
}