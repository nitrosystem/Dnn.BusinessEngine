using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.DashboardAppearance.Models
{
    public class DashboardTemplate 
    {
        public string DashboardType { get; set; }
        public string TemplateName { get; set; }
        public string TemplateImage { get; set; }
        public string TemplatePath { get; set; }
        public string Description { get; set; }
        public JObject BodyOptions { get; set; }
        public IEnumerable<DashboardSkinTheme> Themes{ get; set; }
        public string[] CssFiles { get; set; }
        public string[] JsFiles { get; set; }
    }
}