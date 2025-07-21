using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Core.Models;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels
{
    public class ActionViewModel
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public Guid ModuleId { get; set; }
        public Guid? FieldId { get; set; }
        public Guid? ServiceId { get; set; }
        public string ActionName { get; set; }
        public string ActionType { get; set; }
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
        public int Version { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public int ViewOrder { get; set; }
        public IDictionary<string, object> Settings { get; set; }
        public IEnumerable<ActionResultViewModel> Results { get; set; }
        public IEnumerable<ActionConditionInfo> Conditions { get; set; }
        public IEnumerable<ActionParamInfo> Params { get; set; }
        public string FieldName { get; set; }
        public string CreatedByUserDisplayName { get; set; }
        public string LastModifiedByUserDisplayName { get; set; }
        public string ActionTypeIcon { get; set; }
    }
}