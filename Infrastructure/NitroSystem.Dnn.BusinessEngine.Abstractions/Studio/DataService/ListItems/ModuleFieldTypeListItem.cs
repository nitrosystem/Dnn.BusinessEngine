using System;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ListItems
{
    public class ModuleFieldTypeListItem
    {
        public string FieldType { get; set; }
        public string Title { get; set; }
        public string GroupName { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
        public int ViewOrder { get; set; }
        public int GroupViewOrder { get; set; }
    }
}
