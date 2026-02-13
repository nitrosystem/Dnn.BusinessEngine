using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables
{
    [Table("BusinessEngine_ModuleEventTypes")]
    [Cacheable("BE_ModuleEventTypes_", CacheItemPriority.Default, 20)]
    public class ModuleEventTypeInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid ExtensionId { get; set; }
        public string Component { get; set; }
        public string EventName { get; set; }
        public string Title { get; set; }
        public int TriggerOn { get; set; }
        public int ViewOrder { get; set; }
    }
}
