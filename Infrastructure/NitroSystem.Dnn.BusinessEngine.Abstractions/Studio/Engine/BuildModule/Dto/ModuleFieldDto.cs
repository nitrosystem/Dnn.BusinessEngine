using System;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ModuleBuilder.Models;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Dto
{
    public class ModuleFieldDto
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public string FieldType { get; set; }
        public string FieldName { get; set; }
        public string FieldText { get; set; }
        public string FieldValueProperty { get; set; }
        public string PaneName { get; set; }
        public string Template { get; set; }
        public string TemplatePath { get; set; }
        public string Theme { get; set; }
        public string ThemeCssClass { get; set; }
        public bool CanHaveValue { get; set; }
        public bool IsRequired { get; set; }
        public bool IsGroupField { get; set; }
        public bool IsParent { get; set; }
        public bool HasDataSource { get; set; }
        public string ShowConditions { get; set; }
        public int ViewOrder { get; set; }
        public FieldDataSourceInfo DataSource { get; set; }
        public ModuleFieldGlobalSettings GlobalSettings { get; set; }
        public IDictionary<string, object> Settings { get; set; }
    }
}
