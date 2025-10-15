using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Module
{
    public class ModuleTemplateViewModel
    {
        public Guid Id { get; set; }
        public string Template { get; set; }
        public string Theme { get; set; }
        public string PreloadingTemplate { get; set; }
        public string LayoutTemplate { get; set; }
        public string LayoutCss { get; set; }
    }
}
