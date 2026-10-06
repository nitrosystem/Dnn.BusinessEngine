using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Shared
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
