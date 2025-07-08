using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Api.DTO
{
    public class ModuleTemplateDTO
    {
        public Guid ModuleId { get; set; }
        public string Template { get; set; }
        public string Theme { get; set; }
        public string LayoutTemplate { get; set; }
        public string LayoutCss { get; set; }
    }
}
