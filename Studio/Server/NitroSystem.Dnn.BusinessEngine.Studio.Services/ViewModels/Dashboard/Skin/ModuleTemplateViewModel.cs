using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NitroSystem.Dnn.BusinessEngine.Core.Contract;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Dashboard.Skin
{
    public class ModuleTemplateViewModel :IViewModel
    {
        public Guid Id { get; set; }
        public string TemplateName { get; set; }
        public string TemplateImage { get; set; }
        public string TemplatePath { get; set; }
        public string Description { get; set; }
    }
}