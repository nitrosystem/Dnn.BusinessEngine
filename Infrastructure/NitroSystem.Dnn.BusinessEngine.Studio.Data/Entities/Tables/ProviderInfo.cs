using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables
{
    [Table("BusinessEngine_Providers")]
    [Cacheable("BE_Providers_", CacheItemPriority.Default, 20)]
    public class ProviderInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid ExtensionId { get; set; }
        public string ProviderType { get; set; }
        public string ProviderName { get; set; }
        public string Title { get; set; }
        public string ProviderComponent { get; set; }
        public string BusinessControllerClass { get; set; }
        public string Description { get; set; }
    }
}