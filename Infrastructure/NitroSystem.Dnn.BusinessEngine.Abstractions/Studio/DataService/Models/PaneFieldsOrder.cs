using System;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Models
{
    public class PaneFieldsOrder
    {
        public Guid ModuleId { get; set; }
        public Guid FieldId { get; set; }
        public Guid? ParentId { get; set; }
        public string PaneName { get; set; }
        public IEnumerable<Guid> PaneFieldIds { get; set; }
    }
}
