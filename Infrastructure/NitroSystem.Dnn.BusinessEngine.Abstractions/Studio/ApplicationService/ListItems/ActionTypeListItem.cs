namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ListItems
{
    public class ActionTypeListItem
    {
        public string ActionDomain { get; set; }
        public string ActionType { get; set; }
        public string ActionComponent { get; set; }
        public string Title { get; set; }
        public bool HideModuleBuilder { get; set; }
        public bool IsResultable { get; set; }
        public string DslSuggestions { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
        public int ViewOrder { get; set; }
    }
}
