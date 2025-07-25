using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels
{
    public class DefinedListViewModel
    {
        public Guid Id { get; set; }
        public Guid? ScenarioId { get; set; }
        public Guid? ParentId { get; set; }
        public Guid? FieldId { get; set; }
        public string ListName { get; set; }
        public string ListType { get; set; }
        public bool IsMultiLevelList { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public int ViewOrder { get; set; }
        public IEnumerable<DefinedListItemViewModel> Items { get; set; }
    }
}