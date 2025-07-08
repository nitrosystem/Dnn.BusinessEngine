using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Api.DTO
{
    public class DashboardPageSortDTO
    {
        public Guid DashboardID { get; set; }
        //public DashboardPageInfo MovedPage { get; set; }
        public IEnumerable<Guid> SortedPageIDs { get; set; }
    }
}
