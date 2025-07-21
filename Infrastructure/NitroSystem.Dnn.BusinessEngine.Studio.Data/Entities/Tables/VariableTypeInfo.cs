using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables
{
    [Table("BusinessEngine_VariableTypes")]
    [Cacheable("BE_VariableTypes_", CacheItemPriority.Default, 20)]
    public class VariableTypeInfo : IEntity
    {
        public Guid Id { get; set; }
        public string VariableType { get; set; }
        public int Language { get; set; }
        public string Category { get; set; }
        public bool IsSystemVariable { get; set; }
        public bool SupportCsharp { get; set; }
        public bool SupportSql { get; set; }
        public bool SupportJs { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
        public int ViewOrder { get; set; }
    }
}