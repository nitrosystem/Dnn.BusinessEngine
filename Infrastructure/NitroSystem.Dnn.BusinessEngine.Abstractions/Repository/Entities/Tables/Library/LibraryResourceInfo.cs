using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Library
{
    [Table("BusinessEngine_LibraryResources")]
    [Cacheable("BE_Libraries_Resources_", CacheItemPriority.Default, 20)]
    [Scope("LibraryId")]
    public class LibraryResourceInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid LibraryId { get; set; }
        public int ResourceContentType { get; set; }
        public string ResourcePath { get; set; }
        public int LoadOrder { get; set; }
    }
}