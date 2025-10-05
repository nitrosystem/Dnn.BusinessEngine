﻿using System;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Views
{
    public class ModuleFieldResult
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
        public string TemplatePath { get; set; }
        public string Theme { get; set; }
        public string ThemeCssClass { get; set; }
        public bool CanHaveValue { get; set; }
        public bool IsRequired { get; set; }
        public bool IsGroupField { get; set; }
        public bool HasDataSource { get; set; }
        public string DataSource { get; set; }
        public string ShowConditions { get; set; }
        public string AuthorizationViewField { get; set; }
        public int ViewOrder { get; set; }
    }
}
