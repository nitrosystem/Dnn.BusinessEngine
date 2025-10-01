using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables
{
    [Table("BusinessEngine_ServiceParams")]
    [Cacheable("BE_ServiceParams_", CacheItemPriority.Default, 20)]
    [Scope("ServiceId")]
    public class ServiceParamInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid ServiceId { get; set; }
        public string ParamName { get; set; }
        public string ParamType { get; set; }
        public int ViewOrder { get; set; }
    }
}
