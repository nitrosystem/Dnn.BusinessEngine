using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables
{
    [Table("BusinessEngine_ModuleOutputResources")]
    [Cacheable("BE_ModuleOutputResources_", CacheItemPriority.Default, 20)]
    [Scope("ModuleId")]
    public class ModuleOutputResourceInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public int? SitePageId { get; set; }
        public int ResourceContentType { get; set; }
        public string ResourcePath { get; set; }
        public bool IsSystemResource { get; set; }
        public int LoadOrder { get; set; }
    }
}