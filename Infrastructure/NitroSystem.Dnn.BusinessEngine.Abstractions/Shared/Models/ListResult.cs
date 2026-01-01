using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models
{
    public class ListResult
    {
        public IEnumerable<object> Items { get; set; }
        public int TotalCount { get; set; }
    }
}
