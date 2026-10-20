using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Module
{
    [Table("BusinessEngine_ModuleClientBehaviors")]
    [Cacheable("BE_Modules_ClientBehaviors", CacheItemPriority.Default, 20)]
    [Scope("ModuleId")]
    public class ModuleClientBehaviorInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public Guid? FieldId { get; set; }
        public Guid? ActionId { get; set; }
        public string Title { get; set; }
        public string Event { get; set; }
        public string Script { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public int ViewOrder { get; set; }
    }
}
