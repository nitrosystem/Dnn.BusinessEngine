using NitroSystem.Dnn.BusinessEngine.Studio.Engine.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.Dto
{
    public class BuildModuleDto
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public string ModuleType { get; set; }
        public string ModuleBuilderType { get; set; }
        public string ModuleName { get; set; }
        public string LayoutTemplate { get; set; }
        public string LayoutCss { get; set; }
        public string BuildPath { get; set; }
    }
}
