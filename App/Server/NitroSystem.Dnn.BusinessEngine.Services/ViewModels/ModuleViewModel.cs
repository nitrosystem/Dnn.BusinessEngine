using NitroSystem.Dnn.BusinessEngine.App.Services.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.App.Services.ViewModels
{
    public class ModuleViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public int PortalId { get; set; }
        public int? DnnModuleId { get; set; }
        public ModuleWrapper Wrapper { get; set; }
        public ModuleType ModuleType { get; set; }
        public string ModuleName { get; set; }
        public string ModuleTitle { get; set; }
        public string LayoutTemplate { get; set; }
        public string LayoutCss { get; set; }
        public int Version { get; set; }
        public IDictionary<string, object> Settings { get; set; }
        public string CustomHtml { get; set; }
        public string CustomJs { get; set; }
        public string CustomCss { get; set; }
    }
}