using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables
{
    [Table("BusinessEngine_ModuleFieldTypeLibraries")]
    [Cacheable("BE_ModuleFieldTypes_Libraries_", CacheItemPriority.Default, 20)]
    [Scope("FieldType")]
    public class ModuleFieldTypeLibraryInfo : IEntity
    {
        public Guid Id { get; set; }
        public string FieldType { get; set; }
        public string LibraryName { get; set; }
        public string Version { get; set; }
        public int LoadOrder { get; set; }

    }
}