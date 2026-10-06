using System;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ListItems
{
    public class EntityListItem
    {
        public Guid Id { get; set; }
        public string EntityName { get; set; }
        public string TableName { get; set; }
        public int ViewOrder { get; set; }
        public EntityType EntityType { get; set; }
        public IEnumerable<EntityColumnListItem> Columns { get; set; }
    }
}
