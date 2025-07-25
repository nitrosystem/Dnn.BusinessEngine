using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables
{
    [Table("BusinessEngine_PageResources")]
    [Cacheable("BE_PageResources_", CacheItemPriority.Default, 20)]
    [Scope("ModuleId")]
    public class PageResourceInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public int? DnnPageId { get; set; }
        public bool IsSystemResource { get; set; }
        public bool IsCustomResource { get; set; }
        public string ResourceType { get; set; }
        public string ResourcePath { get; set; }
        public bool IsActive { get; set; }
        public int LoadOrder { get; set; }
    }
}