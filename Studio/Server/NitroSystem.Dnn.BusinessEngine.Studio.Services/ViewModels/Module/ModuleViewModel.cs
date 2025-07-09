using NitroSystem.Dnn.BusinessEngine.Core.Contract;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels
{
    public class ModuleViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public Guid ScenarioId { get; set; }
        public Guid? ParentId { get; set; }
        public int PortalId { get; set; }
        public int? DnnModuleId { get; set; }
        public bool IsSSR { get; set; }
        public ModuleType ModuleType { get; set; }
        public ModuleBuilderType ModuleBuilderType { get; set; }
        public ModuleWrapper Wrapper { get; set; }
        public string ModuleName { get; set; }
        public string ModuleTitle { get; set; }
        public string Template { get; set; }
        public string Theme { get; set; }
        public string LayoutTemplate { get; set; }
        public string LayoutCss { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public int Version { get; set; }
        public int ViewOrder { get; set; }
        public IDictionary<string, object> Settings { get; set; }
        public string CustomHtml { get; set; }
        public string CustomJs { get; set; }
        public string CustomCss { get; set; }
    }
}