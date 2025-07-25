using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables
{
    [Table("BusinessEngineView_ModuleFieldTypeLibraries")]
    [Cacheable("BE_ModuleFieldTypeLibraries_View_", CacheItemPriority.Default, 20)]
    [Scope("FieldType")]
    public class ModuleFieldTypeLibraryView : IEntity
    {
        public Guid Id { get; set; }
        public Guid LibraryId { get; set; }
        public string LibraryName { get; set; }
        public string Logo { get; set; }
        public string Version { get; set; }
        public string FieldType { get; set; }
        public string ResourceType { get; set; }
        public string ResourcePath { get; set; }
        public int LoadOrder { get; set; }
    }
}