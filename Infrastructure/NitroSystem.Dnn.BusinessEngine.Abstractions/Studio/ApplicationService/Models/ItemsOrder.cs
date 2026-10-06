using System;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Models
{
    public class ItemsOrder
    {
        public Guid Id { get; set; }
        public IEnumerable<Guid> SortedIds { get; set; }
    }
}
