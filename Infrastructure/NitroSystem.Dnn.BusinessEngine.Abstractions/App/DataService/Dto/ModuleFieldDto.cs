using System;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Dto
{
    public class ModuleFieldDto 
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public Guid? ParentId { get; set; }
        public string FieldType { get; set; }
        public string FieldName { get; set; }
        public string FieldText { get; set; }
        public string PaneName { get; set; }
        public string FieldValueProperty { get; set; }
        public string ThemeCssClass { get; set; }
        public bool CanHaveValue  { get; set; }
        public bool IsRequired { get; set; }
        public bool IsGroupField { get; set; }
        public bool HasDataSource { get; set; }
        public string ShowConditions { get; set; }
        public IEnumerable<string> AuthorizationViewField { get; set; }
        public ModuleFieldDataSourceResult DataSource { get; set; }
        public IEnumerable<ModuleFieldValueInfo> ConditionalValues { get; set; }
        public IDictionary<string, object> Settings { get; set; }
    }
}