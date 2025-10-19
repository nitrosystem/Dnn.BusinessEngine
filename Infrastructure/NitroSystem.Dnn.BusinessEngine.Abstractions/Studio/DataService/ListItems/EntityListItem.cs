using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ListItems
{
    public class EntityListItem
    {
        public string EntityName { get; set; }
        public string TableName { get; set; }
        public EntityType EntityType { get; set; }
        public int ViewOrder { get; set; }
        public IEnumerable<EntityColumnListItem> Columns { get; set; }

    }
}
