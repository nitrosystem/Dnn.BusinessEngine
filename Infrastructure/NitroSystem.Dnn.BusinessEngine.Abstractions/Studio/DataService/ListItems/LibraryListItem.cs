using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ListItems
{
    public class LibraryListItem
    {
        public string LibraryName { get; set; }
        public string Version { get; set; }
        public string Logo { get; set; }
        public int LoadOrder { get; set; }
        public IEnumerable<LibraryResourceListItem> Resources { get; set; }
    }
}
