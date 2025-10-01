using System;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Module;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Dto
{
   public class UpdateModuleFieldDto
    {
        public ModuleFieldViewModel  Field { get; set; }
        public List<Guid> PaneFieldIds { get; set; }
        public bool ReorderFields { get; set; }
        public int? FieldViewOrder { get; set; }
    }
}
