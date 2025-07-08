using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.DashboardAppearance.Models
{
   public class ModuleTemplate
    {
        public string TemplateName { get; set; }
        public string TemplateImage { get; set; }
        public string TemplatePath { get; set; }
        public string Description { get; set; }
        public IEnumerable<DashboardSkinTheme> Themes { get; set; }
    }
}
