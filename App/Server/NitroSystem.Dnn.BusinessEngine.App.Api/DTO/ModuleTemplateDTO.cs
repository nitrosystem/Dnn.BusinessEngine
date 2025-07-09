using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Api.DTO
{
    public class ModuleTemplateDTO
    {
        public Guid ModuleID { get; set; }
        public Guid TemplateID { get; set; }
        public string Template { get; set; }
        public string Theme { get; set; }
        public string HtmlPath { get; set; }
        public string CssPath { get; set; }
    }
}
