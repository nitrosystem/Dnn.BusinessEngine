using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ListItems
{
    public class LibraryResourceListItem
    {
        public string ResourcePath { get; set; }
        public int LoadOrder { get; set; }
        public ResourceContentType ResourceContentType { get; set; }
    }
}
