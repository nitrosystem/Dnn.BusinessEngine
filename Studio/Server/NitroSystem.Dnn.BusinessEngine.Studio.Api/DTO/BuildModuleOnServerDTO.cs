using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Module.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Api.DTO
{
    public class BuildModuleOnServerDTO
    {
        public Guid ModuleId { get; set; }
        public Guid? ParentID { get; set; }
        public IEnumerable<ModuleFieldViewModel> Fields { get; set; }
    }
}
