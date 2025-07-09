using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

using NitroSystem.Dnn.BusinessEngine.Studio.Data.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contract;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables
{
    [Table("BusinessEngine_LibraryResources")]
    [Cacheable("BE_LibraryResources_", CacheItemPriority.Default, 20)]
    [Scope("LibraryId")]
    public class LibraryResourceInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid LibraryId { get; set; }
        public string ResourceType { get; set; }
        public string ResourcePath { get; set; }
        public int LoadOrder { get; set; }
    }
}