using System;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataServices.ViewModels.Template
{
    public class TemplateThemeViewModel : IViewModel
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
