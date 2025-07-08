using Newtonsoft.Json.Linq;
using NitroSystem.Dnn.BusinessEngine.Studio.ApplicationServices.ViewModels.Dashboard.Skin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.Contract
{
    public interface IDashboardTemplate
    {
         string DashboardType { get; set; }
         string TemplateName { get; set; }
         string TemplateImage { get; set; }
         string TemplatePath { get; set; }
         string Description { get; set; }
         JObject BodyOptions { get; set; }
         IEnumerable<DashboardThemeInfo> Themes { get; set; }
         string[] CssFiles { get; set; }
         string[] JsFiles { get; set; }
    }
}
