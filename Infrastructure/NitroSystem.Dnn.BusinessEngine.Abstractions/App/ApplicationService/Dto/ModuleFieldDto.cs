using System;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Dto
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
        public string HiddenConditions { get; set; }
        public ModuleFieldDataSourceDto DataSource { get; set; }
        public IDictionary<string, object> Settings { get; set; }
    }
}