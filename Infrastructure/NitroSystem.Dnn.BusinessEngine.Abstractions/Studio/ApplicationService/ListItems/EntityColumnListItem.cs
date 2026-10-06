using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ListItems
{
   public class EntityColumnListItem
    {
        public Guid Id { get; set; }
        public string ColumnName { get; set; }
        public string ColumnType { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsIdentity { get; set; }
        public bool AllowNulls { get; set; }
        public int ViewOrder { get; set; }
    }
}
