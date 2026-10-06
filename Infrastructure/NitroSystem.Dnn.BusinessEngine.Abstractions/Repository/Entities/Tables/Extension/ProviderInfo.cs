using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Extension
{
    [Table("BusinessEngine_Providers")]
    [Cacheable("BE_Providers_", CacheItemPriority.Default, 20)]
    public class ProviderInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid ExtensionId { get; set; }
        public int ProviderType { get; set; }
        public string ProviderName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}