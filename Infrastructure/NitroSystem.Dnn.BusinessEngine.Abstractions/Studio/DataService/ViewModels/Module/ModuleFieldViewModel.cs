using System;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ListItems;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Module
{
    public class ModuleFieldViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public Guid? ParentId { get; set; }
        public string FieldType { get; set; }
        public string FieldName { get; set; }
        public string FieldText { get; set; }
        public string FieldValueProperty { get; set; }
        public string PaneName { get; set; }
        public string Template { get; set; }
        public string Theme { get; set; }
        public string ThemeCssClass { get; set; }
        public bool CanHaveValue  { get; set; }
        public bool IsRequired { get; set; }
        public bool IsGroupField { get; set; }
        public bool HasDataSource { get; set; }
        public string ShowConditions { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public int ViewOrder { get; set; }
        public IEnumerable<string> AuthorizationViewField { get; set; }
        public ModuleFieldDataSourceInfo DataSource { get; set; }
        public IEnumerable<ModuleFieldValueInfo> ConditionalValues { get; set; }
        public IEnumerable<ActionListItem> Actions { get; set; }
        public IDictionary<string, object> Settings { get; set; }
    }
}