using System;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Dashboard
{
    public class FieldTypeThemeViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public string ThemeName { get; set; }
        public string ThemeImage { get; set; }
        public string ThemeCssPath { get; set; }
        public string ThemeCssClass { get; set; }
        public bool IsDark { get; set; }
        public string Description { get; set; }
    }
}
