using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using System;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Template
{
    public class TemplateViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public string TemplateName { get; set; }
        public string TemplateImage { get; set; }
        public string TemplatePath { get; set; }
        public string TemplateCssPath { get; set; }
        public string Description { get; set; }
        public int ViewOrder { get; set; }
        public IEnumerable<string> PreviewImages { get; set; }
        public IEnumerable<TemplateThemeViewModel> Themes { get; set; }
    }
}