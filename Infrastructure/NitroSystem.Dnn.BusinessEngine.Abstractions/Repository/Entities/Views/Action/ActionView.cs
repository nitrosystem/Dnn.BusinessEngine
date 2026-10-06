using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Views.Action
{
    [Table("BusinessEngineView_Actions")]
    [Cacheable("BE_Actions_View_", CacheItemPriority.Default, 20)]
    [Scope("ModuleId")]
    public class ActionView : IEntity
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
        public int ExecuteOrder { get; set; }
        public string AuthorizationRunAction { get; set; }
        public string ActionConditionsDsl { get; set; }
        public string BeforeExecuteActionDsl { get; set; }
        public string ActionResultsDsl { get; set; }
        public string Settings { get; set; }
        public string ActionTypeTitle { get; set; }
        public string ActionTypeIcon { get; set; }
        public string ActionTypeDescription { get; set; }
        public string FieldType { get; set; }
        public string FieldName { get; set; }
        public int FieldViewOrder { get; set; }
        public string ServiceType{ get; set; }
        public string ServiceName { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public string CreatedByUserDisplayName { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public string LastModifiedByUserDisplayName { get; set; }
        public int ViewOrder { get; set; }
    }
}