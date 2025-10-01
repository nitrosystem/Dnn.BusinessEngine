using System;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Module
{
   public class ModuleFieldTypeTemplateViewModel
    {
        public Guid Id { get; set; }
        public string FieldType { get; set; }
        public string Parent { get; set; }
        public string TemplateName { get; set; }
        public string TemplateImage { get; set; }
        public string TemplatePath { get; set; }
        public string Description { get; set; }
        public int ViewOrder { get; set; }
    }
}
