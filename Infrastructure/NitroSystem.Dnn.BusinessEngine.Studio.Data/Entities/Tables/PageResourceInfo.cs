using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

using NitroSystem.Dnn.BusinessEngine.Studio.Data.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contract;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables
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