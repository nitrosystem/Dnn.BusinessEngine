using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Dto
{
   public class ModuleLayoutTemplateDto
    {
        public Guid ModuleId { get; set; }
        public string PreloadingTemplate { get; set; }
        public string LayoutTemplate { get; set; }
        public string LayoutCss { get; set; }
    }
}
