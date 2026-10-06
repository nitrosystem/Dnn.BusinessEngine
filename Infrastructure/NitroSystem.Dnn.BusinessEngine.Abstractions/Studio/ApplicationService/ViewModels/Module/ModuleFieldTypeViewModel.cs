using System;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Module
{
    public class ModuleFieldTypeViewModel
    {
        public Guid Id { get; set; }
        public string FieldDomain { get; set; }
        public string GroupName { get; set; }
        public string FieldType { get; set; }
        public string Title { get; set; }
        public string FieldComponent { get; set; }
        public string ComponentParams { get; set; }
        public string FieldJsPath { get; set; }
        public bool IsGroupField { get; set; }
        public bool IsContentField { get; set; }
        public bool CanHaveValue  { get; set; }
        public bool CanHaveAction { get; set; }
        public bool HasDataSource { get; set; }
        public bool IsPopular { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
        public int ViewOrder { get; set; }
        public IEnumerable<ModuleFieldTypeTemplateViewModel> Templates { get; set; }
        public IEnumerable<ModuleFieldTypeThemeViewModel> Themes { get; set; }
        public IDictionary<string, object> DefaultSettings { get; set; }
    }
}
