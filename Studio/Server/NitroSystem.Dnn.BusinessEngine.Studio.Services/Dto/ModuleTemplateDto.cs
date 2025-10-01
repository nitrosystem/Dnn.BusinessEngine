using System;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Dto
{
    public class ModuleTemplateDto
    {
        public Guid Id { get; set; }
        public string Template { get; set; }
        public string Theme { get; set; }
        public string PreloadingTemplate { get; set; }
        public string LayoutTemplate { get; set; }
        public string LayoutCss { get; set; }
    }
}
