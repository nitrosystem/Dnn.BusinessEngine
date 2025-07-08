using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.Dto
{
    public class ModuleDto
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public Guid? TemplateId { get; set; }
        public int PortalId { get; set; }
        public int? DnnModuleId { get; set; }
        public string ScenarioName { get; set; }
        public string Wrapper { get; set; }
        public string ModuleType { get; set; }
        public string ModuleBuilderType { get; set; }
        public string ModuleName { get; set; }
        public string ParentModuleName { get; set; }
        public string Template { get; set; }
        public string Theme { get; set; }
        public string LayoutTemplate { get; set; }
        public string LayoutCss { get; set; }
        public bool IsSSR { get; set; }
        public string Settings { get; set; }
    }
}
