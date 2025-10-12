using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables
{
    [Table("BusinessEngineView_ModuleCustomLibraryResources")]
    [Cacheable("BE_ModuleCustomLLibraryResources_View_", CacheItemPriority.Default, 20)]
    [Scope("ModuleId")]
    public class ModuleCustomLibraryResourceView : IEntity
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public Guid LibraryId { get; set; }
        public int ResourceContentType { get; set; }
        public string ResourcePath { get; set; }
        public int LoadOrder { get; set; }
    }
}