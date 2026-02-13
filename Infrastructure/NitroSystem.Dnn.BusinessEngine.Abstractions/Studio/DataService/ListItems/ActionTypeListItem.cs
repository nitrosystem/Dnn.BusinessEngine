using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ListItems
{
    public class ActionTypeListItem
    {
        public string ActionType { get; set; }
        public string ActionComponent { get; set; }
        public string Title { get; set; }
        public string GroupName { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
        public int GroupViewOrder { get; set; }
        public int ViewOrder { get; set; }
    }
}
