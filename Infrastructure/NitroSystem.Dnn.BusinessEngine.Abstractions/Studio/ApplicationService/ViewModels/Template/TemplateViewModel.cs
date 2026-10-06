using System;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Template
{
    public class TemplateViewModel
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public string TemplateName { get; set; }
        public string TemplateImage { get; set; }
        public string TemplatePath { get; set; }
        public string TemplateCssPath { get; set; }
        public string Description { get; set; }
        public IEnumerable<string> PreviewImages { get; set; }
        public IEnumerable<TemplateThemeViewModel> Themes { get; set; }
    }
}