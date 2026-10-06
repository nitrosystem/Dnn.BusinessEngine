using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Module
{
    [Table("BusinessEngine_ModuleOutputResources")]
    [Cacheable("BE_Modules_OutputResources_", CacheItemPriority.Default, 20)]
    [Scope("ModuleId")]
    public class ModuleOutputResourceInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public int? SitePageId { get; set; }
        public int ResourceContentType { get; set; }
        public string ResourcePath { get; set; }
        public int LoadOrder { get; set; }
    }
}