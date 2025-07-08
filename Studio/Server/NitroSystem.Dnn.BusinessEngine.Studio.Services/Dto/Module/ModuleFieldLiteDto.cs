using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Dto.Module
{
    public class ModuleFieldLiteDto
    {
        public Guid Id { get; set; }
        public string FieldName { get; set; }
        public string FieldType { get; set; }
        public string FieldText { get; set; }
    }
}
