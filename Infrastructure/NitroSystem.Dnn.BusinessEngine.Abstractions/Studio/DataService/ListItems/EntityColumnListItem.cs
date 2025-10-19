using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ListItems
{
   public class EntityColumnListItem
    {
        public Guid Id { get; set; }
        public string ColumnName { get; set; }
        public string ColumnType { get; set; }
        public bool AllowNulls { get; set; }
        public string DefaultValue { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsUnique { get; set; }
        public bool IsComputedColumn { get; set; }
        public bool IsIdentity { get; set; }
        public string Description { get; set; }
        public int ViewOrder { get; set; }
    }
}
