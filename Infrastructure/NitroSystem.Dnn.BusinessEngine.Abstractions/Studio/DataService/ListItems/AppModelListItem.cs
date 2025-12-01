using System;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ListItems
{
    public class AppModelListItem
    {
        public Guid Id { get; set; }
        public string ModelName { get; set; }
        public int ViewOrder { get; set; }
        public AppModelType ModelType { get; set; }
        public IEnumerable<AppModelPropertyListItem> Properties { get; set; }
    }
}
