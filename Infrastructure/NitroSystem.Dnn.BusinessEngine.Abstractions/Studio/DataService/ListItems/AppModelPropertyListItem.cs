using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ListItems
{
    public class AppModelPropertyListItem
    {
        public Guid Id { get; set; }
        public Guid AppModelId { get; set; }
        public string PropertyName { get; set; }
        public string PropertyType { get; set; }
        public int ViewOrder { get; set; }
    }
}
