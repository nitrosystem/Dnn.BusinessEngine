using System;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Models
{
    public class DashboardPagesOrder
    {
        public Guid Id { get; set; }
        public IEnumerable<Guid> SortedPageIds { get; set; }
    }
}
