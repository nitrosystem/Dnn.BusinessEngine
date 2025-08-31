using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Dto
{
    public class SortPaneFieldsDto
    {
        public Guid ModuleId { get; set; }
        public Guid FieldId { get; set; }
        public string PaneName { get; set; }
        public IEnumerable<Guid> PaneFieldIds { get; set; }
    }
}
