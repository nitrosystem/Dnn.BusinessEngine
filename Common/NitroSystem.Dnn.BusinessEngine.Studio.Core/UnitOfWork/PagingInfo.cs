using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork
{
    public class PagingInfo
    {
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }

        public PagingInfo(int? totalCount, int pageSize, int pageIndex)
        {
            TotalCount = totalCount ?? 0;
            PageSize = pageSize;
            PageIndex = pageIndex;
        }

        public bool HasNextPage => PageIndex < PageCount;
        public bool HasPreviousPage => PageIndex > 1;
        public bool IsFirstPage => PageIndex == 1;
        public bool IsLastPage => PageIndex == PageCount;
        public int PageCount => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
