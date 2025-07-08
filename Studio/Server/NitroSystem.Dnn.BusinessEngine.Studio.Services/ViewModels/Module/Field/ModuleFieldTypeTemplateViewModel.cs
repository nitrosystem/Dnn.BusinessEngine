using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Module.Field
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
