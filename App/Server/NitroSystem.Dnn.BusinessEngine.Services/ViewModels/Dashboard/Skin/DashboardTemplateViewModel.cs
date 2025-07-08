using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NitroSystem.Dnn.BusinessEngine.Core.Contract;

namespace NitroSystem.Dnn.BusinessEngine.App.Services.ViewModels.Dashboard.Skin
{
    public class DashboardTemplateViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public string DashboardType { get; set; }
        public string TemplateName { get; set; }
        public string TemplateImage { get; set; }
        public string TemplatePath { get; set; }
        public string Description { get; set; }
        public JObject BodyOptions { get; set; }
        public IEnumerable<DashboardThemeViewModel> Themes { get; set; }
        public string[] CssFiles { get; set; }
        public string[] JsFiles { get; set; }
    }
}