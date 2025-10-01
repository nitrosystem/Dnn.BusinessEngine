using System;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Module
{
    public class ModuleFieldTypeThemeViewModel
    {
        public Guid Id { get; set; }
        public string FieldType { get; set; }
        public string TemplateName { get; set; }
        public string ThemeName { get; set; }
        public string ThemeImage { get; set; }
        public string ThemeCssPath { get; set; }
        public string ThemeCssClass { get; set; }
        public bool IsDark { get; set; }
        public string Description { get; set; }
        public int ViewOrder { get; set; }
    }
}
