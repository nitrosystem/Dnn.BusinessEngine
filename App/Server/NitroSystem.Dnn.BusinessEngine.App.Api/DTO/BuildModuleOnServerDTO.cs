using NitroSystem.Dnn.BusinessEngine.App.Services.ViewModels.Module.Field;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Api.DTO
{
    public class BuildModuleOnServerDTO
    {
        public Guid ModuleID { get; set; }
        public Guid? ParentID { get; set; }
        public IEnumerable<ModuleFieldViewModel> Fields { get; set; }
    }
}
