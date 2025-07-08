using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Api.DTO
{
    public class DashboardPageSortDTO
    {
        public Guid DashboardId { get; set; }
        //public DashboardPageInfo MovedPage { get; set; }
        public IEnumerable<Guid> SortedPageIds { get; set; }
    }
}
