using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Module.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Dto
{
   public class ModuleFieldUpdateDto
    {
        public ModuleFieldViewModel  Field { get; set; }
        public List<Guid> PaneFieldIds { get; set; }
        public bool ReorderFields { get; set; }
        public int? FieldViewOrder { get; set; }
    }
}
