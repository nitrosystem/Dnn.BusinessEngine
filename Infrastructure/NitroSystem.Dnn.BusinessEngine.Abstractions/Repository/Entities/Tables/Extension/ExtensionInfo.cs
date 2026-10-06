using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Extension
{
    [Table("BusinessEngine_Extensions")]
    [Cacheable("BE_Extensions_", CacheItemPriority.Default, 20)]
    public class ExtensionInfo : IEntity
    {
        public Guid Id { get; set; }
        public int ExtensionType { get; set; }
        public string ExtensionName { get; set; }
        public string Title { get; set; }
        public string FolderName { get; set; }
        public string Version { get; set; }
        public string Logo { get; set; }
        public string Owner { get; set; }
        public string Email { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
    }
}