using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables
{
    [Table("BusinessEngine_ActionParams")]
    [Cacheable("BE_Actions_Params_", CacheItemPriority.Default, 20)]
    [Scope("ActionId")]
    public class ActionParamInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid ActionId { get; set; }
        public string ParamName { get; set; }
        public string ParamValue { get; set; }
        public int ViewOrder { get; set; }
    }
}
