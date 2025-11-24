using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables
{
    [Table("BusinessEngine_Actions")]
    [Cacheable("BE_Actions_", CacheItemPriority.Default, 20)]
    [Scope("ModuleId")]
    public class ActionInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public Guid? ParentId { get; set; }
        public Guid? ServiceId { get; set; }
        public Guid? FieldId { get; set; }
        public string ActionType { get; set; }
        public string ActionName { get; set; }
        public string Event { get; set; }
        public int? ParentActionTriggerCondition { get; set; }
        public bool ExecuteInClientSide { get; set; }
        public bool SetCache { get; set; }
        public bool ClearCache { get; set; }
        public string CacheKey { get; set; }
        public string Preconditions { get; set; }
        public string Conditions { get; set; }
        public string AuthorizationRunAction { get; set; }
        public string Settings { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public int ViewOrder { get; set; }
    }
}