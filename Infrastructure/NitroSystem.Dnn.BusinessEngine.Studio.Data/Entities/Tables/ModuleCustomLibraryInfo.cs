using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables
{
    [Table("BusinessEngine_ModuleCustomLibraries")]
    [Cacheable("BE_ModuleCustomLibraries_", CacheItemPriority.Default, 20)]
    [Scope("ModuleId")]
    public class ModuleCustomLibraryInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public string LibraryName { get; set; }
        public string Version { get; set; }
        public int LoadOrder { get; set; }
    }
}