using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Template
{
    public class TemplateThemeViewModel
    {
        public Guid Id { get; set; }
        public Guid TemplateId { get; set; }
        public string ThemeName { get; set; }
        public string ThemeImage { get; set; }
        public string ThemeCssPath { get; set; }
        public string ThemeCssClass { get; set; }
        public int ViewOrder { get; set; }
    }
}
