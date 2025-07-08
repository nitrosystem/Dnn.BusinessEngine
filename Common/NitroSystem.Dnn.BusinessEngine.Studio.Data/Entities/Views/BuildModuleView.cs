using NitroSystem.Dnn.BusinessEngine.Core.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views
{
    public class BuildModuleView : IEntity
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public int? DnnModuleId { get; set; }
        public string ModuleType { get; set; }
        public string ModuleBuilderType { get; set; }
        public string ModuleName { get; set; }
        public string LayoutTemplate { get; set; }
        public string LayoutCss { get; set; }
        public string BuildPath { get; set; }
    }
}
