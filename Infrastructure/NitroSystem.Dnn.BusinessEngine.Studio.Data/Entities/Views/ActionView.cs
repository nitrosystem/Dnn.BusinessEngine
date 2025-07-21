using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views
{
    [Table("BusinessEngineView_Actions")]
    [Cacheable("BE_Actions_View_", CacheItemPriority.Default, 20)]
    [Scope("ModuleId")]
    public class ActionView : IEntity
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public Guid? ParentId { get; set; }
        public Guid? FieldId { get; set; }
        public Guid? ServiceId { get; set; }
        public string ActionName { get; set; }
        public string ActionType { get; set; }
        public string ActionTypeIcon { get; set; }
        public string ActionTypeTitle { get; set; }
        public string Event { get; set; }
        public byte? ParentResultStatus { get; set; }
        public bool IsServerSide { get; set; }
        public bool RunChildsInServerSide { get; set; }
        public bool HasPreScript { get; set; }
        public bool HasPostScript { get; set; }
        public bool DisableConditionForPreScript { get; set; }
        public bool CheckConditionsInClientSide { get; set; }
        public string PreScript { get; set; }
        public string PostScript { get; set; }
        public string Settings { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public int ViewOrder { get; set; }
    }
}